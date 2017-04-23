using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Protoplasm.PointedIntervals
{
    /// <summary>
    /// 
    /// </summary>
    public static class PointedIntervalsContainerExtender
    {
        private static bool _tested;

        

        /// <summary>
        /// 
        /// </summary>
        public static void Test()
        {
            if(_tested)
                return;

            _tested = true;

            var forInitSelfTest = typeof(Point<decimal>);
            TestIC();
        }

        class IntIntInterval : PointedInterval<int, int>
        {
            public IntIntInterval(Point<int> left = null, Point<int> right = null, int data = 0) : base(left, right, data)
            {
            }
        }
        class IntIntIntervals : PointedIntervalsContainer<IntIntInterval, int, int>
        {
            public IntIntIntervals() : base((left, right, data) => new IntIntInterval(left, right, data), (a,b) => a+b, (a, b) => a - b)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void TestIC()
        {

            var value = 1;

            PointedIntervalsContainerBase<int, int> container = new IntIntIntervals();

            // ========================================================================================
            // "точка" = интервал у которого правый и левый кра€ равны и включены в интервал - [x,x] 
            // ========================================================================================
            // пытаемс€ задать (исключить/включить) некорректные точки:

            container.ExecuteWithException<int, int, Exception>(x => x.Exclude(10, 10, 1, false, true));
            container.ExecuteWithException<int, int, Exception>(x => x.Exclude(10, 10, 1, false, false));
            container.ExecuteWithException<int, int, Exception>(x => x.Exclude(10, 10, 1, true, false));

            container.ExecuteWithException<int, int, Exception>(x => x.Include(10, 10, 1, false, true));
            container.ExecuteWithException<int, int, Exception>(x => x.Include(10, 10, 1, false, false));
            container.ExecuteWithException<int, int, Exception>(x => x.Include(10, 10, 1, true, false));


            // ========================================================================================
            // "сложение"
            // ========================================================================================

            container.Execute
                (
                    x => x.Include(0, 10, value), //                                (,) + [0,10](1) =
                    x => x.Length == 1 && x[0].Check(value, 0, true, 10, true) //   [0,10](1)
                );

            container.Execute
                (
                    x => x.Include(0, 10, value), //                                [0,10](1) + [0,10](1) = 
                    x => x.Length == 1 && x[0].Check(2*value, 0, true, 10, true) // [0,10](2)
                );

            container.Execute
                (
                    x => x.Include(0, 10, value, false, false), //          [0, 10](2) + (0,10)(1) =
                    x => x.Length == 3
                         && x[0].Check(2*value, 0, true, 0, true) //        [0,0](2),
                         && x[1].Check(3*value, 0, false, 10, false) //     (0,10)(3),
                         && x[2].Check(2*value, 10, true, 10, true) //      [10,10](2)
                );


            // ========================================================================================
            // "сложение" с точкой
            // ========================================================================================

            container.Execute
                (
                    x => x.Include(5, 5, value), //                     (0,10)(3) + [5,5](1) =
                    x => x.Length == 5
                         && x[1].Check(3*value, 0, false, 5, false) //  (0,5)(3),
                         && x[2].Check(4*value, 5, true, 5, true) //    [5,5](4),
                         && x[3].Check(3*value, 5, false, 10, false) // (5,10)(3),
                );

            // ========================================================================================
            // если соседние интервал€ имеют одно и то же значение данных, то они объедин€ютс€
            // ========================================================================================
            new IntIntIntervals()
                .Execute
                (
                    x => x.Include(0, 5, value, true, false).Include(5, 10, value, true, false), // [0, 5)(1) + [5,10)(1) = 
                    x => x.Length == 1 && x[0].Check(1*value, 0, true, 10, false) //                [0, 10)(1)
                );


            new IntIntIntervals()
                .Execute
                (
                    x => x.Include(0, 5, value, false, true).Include(5, 10, value, false, true), // (0, 5](1) + (5,10](1) = 
                    x => x.Length == 1 && x[0].Check(1*value, 0, false, 10, true) //                (0, 10](1)
                );
            
            // ========================================================================================



            //var ic = new PointedIntervalsContainer<decimal, IDictionary<string, decimal>>((a, b) => ProcessDict(a, b, (arg1, arg2) => arg1 + arg2), (a, b) => ProcessDict(a, b, (arg1, arg2) => arg1 - arg2), DictToStr);

            //ic
            //    .Include(0, 100, new Dictionary<string, decimal> { { "a", 10 } })
            //    .Include(20, 30, new Dictionary<string, decimal> { { "a", 5 } })
            //    .Exclude(-10, 50, new Dictionary<string, decimal> { { "a", 15 } })
            //    ;
        }

        static bool Check(this PointedInterval<int, int> x, int data, int? left, bool leftIncluded, int? right, bool rightIncluded)
        {
            return x.Data == data && x.Left.Included == leftIncluded && x.Left == left && x.Right.Included == rightIncluded && x.Right == right;
        }

        static void Execute<TB, TD>(this PointedIntervalsContainerBase<TB, TD> container, Action<PointedIntervalsContainerBase<TB, TD>> action, Func<PointedInterval<TB, TD>[], bool> check = null)
            where TB : struct, IComparable<TB>
        {
            try
            {
                action(container);

                if (check == null)
                    return;

                var result = check(container.ToArray());
                Debug.Assert(result);
            }
            catch (Exception ex)
            {
                throw new Exception("!!! самотестирование провалено !!!", ex);
            }
        }
        static void ExecuteWithException<TB, TD, TException>(this PointedIntervalsContainerBase<TB, TD> container, Action<PointedIntervalsContainerBase<TB, TD>> action, Func<TException, bool> checkException = null)
            where TException : Exception
            where TB : struct, IComparable<TB>
        {
            try
            {
                try
                {
                    action(container);
                    Debug.Fail("ожидалось исключение");
                }
                catch (Exception ex)
                {
                    if (checkException != null)
                    {
                        if (ex is TException)
                        {
                            if (checkException((TException) ex))
                            {
                                return;
                            }

                            Debug.Fail($"проверка исключени€ через вызов [{nameof(checkException)}] не пройдена.");
                        }
                        else
                            Debug.Fail("проверка исключени€ не пройдена - ожидалс€ другой тип исключени€");
                    }
                    else
                    {
                        if (ex.GetType() == typeof(TException))
                            return;

                        Debug.Fail("проверка исключени€ не пройдена - ожидалс€ другой тип исключени€");
                    }
                }
            }
            catch(Exception exx)
            {
                throw new Exception("!!! самотестирование провалено !!!", exx);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static string DictToStr<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> data)
        {
            return data == null ? "" : string.Join(", ", data.Select(x => $"'{x.Key}'={x.Value}"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="processValues"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> ProcessDict<TKey, TValue>(
            IDictionary<TKey, TValue> a,
            IDictionary<TKey, TValue> b,
            Func<TValue, TValue, TValue> processValues)
        {
            a = a ?? new Dictionary<TKey, TValue>();
            b = b ?? new Dictionary<TKey, TValue>();

            var keys = a.Keys.Union(b.Keys);
            var pairs = keys
                .ToDictionary
                (
                    x => x,
                    x => processValues(a.ContainsKey(x) ? a[x] : default(TValue), b.ContainsKey(x) ? b[x] : default(TValue))
                )
                .Where(x => !Equals(x.Value, default(TValue)))
                .ToArray();

            return pairs.Length > 0
                       ? pairs.ToDictionary(x => x.Key, x => x.Value)
                       : null
                ;
        }
    }
}