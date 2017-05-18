using System;
using System.Diagnostics;

namespace Protoplasm.Utils
{
    public class ArithmeticAdapterInitializer
    {
        internal static readonly object Locker = new object();
        internal static bool DataAdapterSelfTested;
        internal static bool DataAdapterDefaultsInited;

        public static void Test()
        {


#if SELFTEST
            lock (Locker)
            {
                if (DataAdapterSelfTested)
                    return;

#endif


            var negativeInfinity = ArithmeticAdapter<int>.NegativeInfinity;
            var positiveInfinity = ArithmeticAdapter<int>.PositiveInfinity;
            var undefined = ArithmeticAdapter<int>.Undefined;
            var def = default(ArithmeticAdapter<int>);
            Debug.Assert(def == ArithmeticAdapter<int>.Undefined);

            ArithmeticAdapter<int> one = 1;
            ArithmeticAdapter<int> zero = 0;

            Debug.Assert(zero == ArithmeticAdapter<int>.Zero);


            //---

            Debug.Assert(-positiveInfinity == negativeInfinity);
            Debug.Assert(-negativeInfinity == positiveInfinity);
            Debug.Assert(-undefined == undefined);
            Debug.Assert(-one == -1);
            Debug.Assert(-zero == zero);

            //---

            Debug.Assert(positiveInfinity > ArithmeticAdapter<int>.PositiveInfinity == false);
            Debug.Assert(positiveInfinity > negativeInfinity == true);
            Debug.Assert(positiveInfinity > undefined == false);
            Debug.Assert(positiveInfinity > one == true);

            Debug.Assert(negativeInfinity > ArithmeticAdapter<int>.NegativeInfinity == false);
            Debug.Assert(negativeInfinity > positiveInfinity == false);
            Debug.Assert(negativeInfinity > undefined == false);
            Debug.Assert(negativeInfinity > one == false);

            Debug.Assert(undefined > ArithmeticAdapter<int>.Undefined == false);
            Debug.Assert(undefined > positiveInfinity == false);
            Debug.Assert(undefined > negativeInfinity == false);
            Debug.Assert(undefined > one == false);

            //---


            Debug.Assert(positiveInfinity < ArithmeticAdapter<int>.PositiveInfinity == false);
            Debug.Assert(positiveInfinity < negativeInfinity == false);
            Debug.Assert(positiveInfinity < undefined == false);
            Debug.Assert(positiveInfinity < one == false);

            Debug.Assert(negativeInfinity < ArithmeticAdapter<int>.NegativeInfinity == false);
            Debug.Assert(negativeInfinity < positiveInfinity == true);
            Debug.Assert(negativeInfinity < undefined == false);
            Debug.Assert(negativeInfinity < one == true);

            Debug.Assert(undefined < ArithmeticAdapter<int>.Undefined == false);
            Debug.Assert(undefined < positiveInfinity == false);
            Debug.Assert(undefined < negativeInfinity == false);
            Debug.Assert(undefined < one == false);

            //---

            //---

            Debug.Assert(positiveInfinity >= ArithmeticAdapter<int>.PositiveInfinity == true);
            Debug.Assert(positiveInfinity >= negativeInfinity == true);
            Debug.Assert(positiveInfinity >= undefined == false);
            Debug.Assert(positiveInfinity >= one == true);

            Debug.Assert(negativeInfinity >= ArithmeticAdapter<int>.NegativeInfinity == true);
            Debug.Assert(negativeInfinity >= positiveInfinity == false);
            Debug.Assert(negativeInfinity >= undefined == false);
            Debug.Assert(negativeInfinity >= one == false);

            Debug.Assert(undefined >= ArithmeticAdapter<int>.Undefined == true);
            Debug.Assert(undefined >= positiveInfinity == false);
            Debug.Assert(undefined >= negativeInfinity == false);
            Debug.Assert(undefined >= one == false);

            //---


            Debug.Assert(positiveInfinity <= ArithmeticAdapter<int>.PositiveInfinity == true);
            Debug.Assert(positiveInfinity <= negativeInfinity == false);
            Debug.Assert(positiveInfinity <= undefined == false);
            Debug.Assert(positiveInfinity <= one == false);

            Debug.Assert(negativeInfinity <= ArithmeticAdapter<int>.NegativeInfinity == true);
            Debug.Assert(negativeInfinity <= positiveInfinity == true);
            Debug.Assert(negativeInfinity <= undefined == false);
            Debug.Assert(negativeInfinity <= one == true);

            Debug.Assert(undefined <= ArithmeticAdapter<int>.Undefined == true);
            Debug.Assert(undefined <= positiveInfinity == false);
            Debug.Assert(undefined <= negativeInfinity == false);
            Debug.Assert(undefined <= one == false);

            //---

            //---

            Debug.Assert(positiveInfinity == ArithmeticAdapter<int>.PositiveInfinity == true);
            Debug.Assert(positiveInfinity == negativeInfinity == false);
            Debug.Assert(positiveInfinity == undefined == false);
            Debug.Assert(positiveInfinity == one == false);

            Debug.Assert(negativeInfinity == ArithmeticAdapter<int>.NegativeInfinity == true);
            Debug.Assert(negativeInfinity == positiveInfinity == false);
            Debug.Assert(negativeInfinity == undefined == false);
            Debug.Assert(negativeInfinity == one == false);

            Debug.Assert(undefined == ArithmeticAdapter<int>.Undefined == true);
            Debug.Assert(undefined == positiveInfinity == false);
            Debug.Assert(undefined == negativeInfinity == false);
            Debug.Assert(undefined == one == false);

            //---


            Debug.Assert(positiveInfinity != ArithmeticAdapter<int>.PositiveInfinity == false);
            Debug.Assert(positiveInfinity != negativeInfinity == true);
            Debug.Assert(positiveInfinity != undefined == true);
            Debug.Assert(positiveInfinity != one == true);

            Debug.Assert(negativeInfinity != ArithmeticAdapter<int>.NegativeInfinity == false);
            Debug.Assert(negativeInfinity != positiveInfinity == true);
            Debug.Assert(negativeInfinity != undefined == true);
            Debug.Assert(negativeInfinity != one == true);

            Debug.Assert(undefined != ArithmeticAdapter<int>.Undefined == false);
            Debug.Assert(undefined != positiveInfinity == true);
            Debug.Assert(undefined != negativeInfinity == true);
            Debug.Assert(undefined != one == true);

            //---

            Debug.Assert(positiveInfinity + positiveInfinity == positiveInfinity);
            Debug.Assert(positiveInfinity + negativeInfinity == undefined);
            Debug.Assert(positiveInfinity + undefined == undefined);

            Debug.Assert(negativeInfinity + negativeInfinity == negativeInfinity);
            Debug.Assert(negativeInfinity + positiveInfinity == undefined);
            Debug.Assert(negativeInfinity + undefined == undefined);

            Debug.Assert(undefined + undefined == undefined);
            Debug.Assert(undefined + positiveInfinity == undefined);
            Debug.Assert(undefined + negativeInfinity == undefined);


            //---

            Debug.Assert(positiveInfinity - positiveInfinity == undefined);
            Debug.Assert(positiveInfinity - negativeInfinity == positiveInfinity);
            Debug.Assert(positiveInfinity - undefined == undefined);

            Debug.Assert(negativeInfinity - negativeInfinity == undefined);
            Debug.Assert(negativeInfinity - positiveInfinity == negativeInfinity);
            Debug.Assert(negativeInfinity - undefined == undefined);

            Debug.Assert(undefined - undefined == undefined);
            Debug.Assert(undefined - positiveInfinity == undefined);
            Debug.Assert(undefined - negativeInfinity == undefined);

            //---

            Debug.Assert(positiveInfinity.Min(positiveInfinity) == positiveInfinity);
            Debug.Assert(positiveInfinity.Min(negativeInfinity) == negativeInfinity);
            Debug.Assert(positiveInfinity.Min(undefined) == undefined);

            Debug.Assert(negativeInfinity.Min(negativeInfinity) == negativeInfinity);
            Debug.Assert(negativeInfinity.Min(positiveInfinity) == negativeInfinity);
            Debug.Assert(negativeInfinity.Min(undefined) == undefined);

            Debug.Assert(undefined.Min(undefined) == undefined);
            Debug.Assert(undefined.Min(positiveInfinity) == undefined);
            Debug.Assert(undefined.Min(negativeInfinity) == undefined);

            //---

            Debug.Assert(positiveInfinity.Max(positiveInfinity) == positiveInfinity);
            Debug.Assert(positiveInfinity.Max(negativeInfinity) == positiveInfinity);
            Debug.Assert(positiveInfinity.Max(undefined) == undefined);

            Debug.Assert(negativeInfinity.Max(negativeInfinity) == negativeInfinity);
            Debug.Assert(negativeInfinity.Max(positiveInfinity) == positiveInfinity);
            Debug.Assert(negativeInfinity.Max(undefined) == undefined);

            Debug.Assert(undefined.Max(undefined) == undefined);
            Debug.Assert(undefined.Max(positiveInfinity) == undefined);
            Debug.Assert(undefined.Max(negativeInfinity) == undefined);

            //---

            Debug.Assert(one + positiveInfinity == positiveInfinity);
            Debug.Assert(positiveInfinity + one == positiveInfinity);

            Debug.Assert(one + negativeInfinity == negativeInfinity);
            Debug.Assert(negativeInfinity + one == negativeInfinity);

            Debug.Assert(one + undefined == undefined);
            Debug.Assert(undefined + one == undefined);

            //---

            Debug.Assert(one - positiveInfinity == negativeInfinity);
            Debug.Assert(positiveInfinity - one == positiveInfinity);

            Debug.Assert(one - negativeInfinity == positiveInfinity);
            Debug.Assert(negativeInfinity - one == negativeInfinity);

            Debug.Assert(one - undefined == undefined);
            Debug.Assert(undefined - one == undefined);

            //--

            Debug.Assert(zero + one == 1);
            Debug.Assert(zero + one == one);

            Debug.Assert(one > 0);
            Debug.Assert(one >= 0);
            Debug.Assert(one != 0);
            Debug.Assert(!(one == 0));
            Debug.Assert(!(one <= 0));
            Debug.Assert(!(one < 0));

            ArithmeticAdapter<int> minus_one;
            Debug.Assert((minus_one = one - 2) == -1);
            Debug.Assert(!(minus_one > 0));
            Debug.Assert(!(minus_one >= 0));
            Debug.Assert(minus_one != 0);
            Debug.Assert(!(minus_one == 0));
            Debug.Assert(minus_one <= 0);
            Debug.Assert(minus_one < 0);


            Debug.Assert(!(minus_one + one > 0));
            Debug.Assert(minus_one + one >= 0);
            Debug.Assert(!(minus_one + one != 0));
            Debug.Assert(minus_one + one == 0);
            Debug.Assert(minus_one + one <= 0);
            Debug.Assert(!(minus_one + one < 0));

            Debug.Assert(!(minus_one + one > zero));
            Debug.Assert(minus_one + one >= zero);
            Debug.Assert(!(minus_one + one != zero));
            Debug.Assert(minus_one + one == zero);
            Debug.Assert(minus_one + one <= zero);
            Debug.Assert(!(minus_one + one < zero));

#if SELFTEST
               DataAdapterSelfTested = true;
            }
#endif

        }

        public static void InitDefaults()
        {
            lock (Locker)
            {
                if (DataAdapterDefaultsInited)
                {
                    return;
                }

                ArithmeticAdapter<int>.Add = (a, b) => a + b;
                ArithmeticAdapter<int>.Subst = (a, b) => a - b;

                ArithmeticAdapter<long>.Add = (a, b) => a + b;
                ArithmeticAdapter<long>.Subst = (a, b) => a - b;

                //ArithmeticAdapter<byte>.Add = (a, b) => a + b;
                //ArithmeticAdapter<byte>.Subst = (a, b) => a - b;

                //ArithmeticAdapter<short>.Add = (a, b) => a + b;
                //ArithmeticAdapter<short>.Subst = (a, b) => a - b;

                ArithmeticAdapter<double>.Add = (a, b) => a + b;
                ArithmeticAdapter<double>.Subst = (a, b) => a - b;

                ArithmeticAdapter<decimal>.Add = (a, b) => a + b;
                ArithmeticAdapter<decimal>.Subst = (a, b) => a - b;

                ArithmeticAdapter<float>.Add = (a, b) => a + b;
                ArithmeticAdapter<float>.Subst = (a, b) => a - b;

                ArithmeticAdapter<TimeSpan>.Add = (a, b) => a + b;
                ArithmeticAdapter<TimeSpan>.Subst = (a, b) => a - b;

#if SELFTEST
                Test();
#endif
                DataAdapterDefaultsInited = true;
            }
        }
    }
}