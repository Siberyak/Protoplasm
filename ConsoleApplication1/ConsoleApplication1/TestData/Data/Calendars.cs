using System;
using System.Collections.Generic;
using System.Linq;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils;

namespace ConsoleApplication1.TestData
{
    public interface IAbstractCalendar
    {
        IAbstractCalendar[] FullChain();
    }

    public static class Calendars<TTime, TDuration>
        where TTime : struct, IComparable<TTime> where TDuration : struct, IComparable<TDuration>
    {
        public static Func<TTime?, TTime?, TDuration?> GetOffset;
    }

    public static class Calendars<TTime, TDuration, TData>
        where TTime : struct, IComparable<TTime>
        where TDuration : struct, IComparable<TDuration>
    {
        public interface ICalendar : IAbstractCalendar
        {
            IEnumerable<CalendarItem> Get(TTime from, TTime to);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public delegate TData Addition(TData a, TData b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public delegate TData Substraction(TData a, TData b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public delegate string ToDebugString(TData data);


        private class CalendarItems : PointedIntervalsContainer<CalendarItem, TTime, TData>, ICalendarItems
        {
            public CalendarItems(Addition includeData, Substraction excludeData, ToDebugString toDebugString = null)
                : base
                    (
                    (left, right, data) => new CalendarItem(left, right, data),
                    (a, b) => includeData(a, b),
                    (a, b) => excludeData(a, b),
                    toDebugString != null
                        ? data => toDebugString(data)
                        : default(DataToString)
                    )
            {
            }


            void ICalendarItems.Include(Point<TTime> left, Point<TTime> right, TData data)
            {
                base.Include(left, right, data);
            }

            void ICalendarItems.Include(TTime? left, TTime? right, TData data, bool leftIncluded, bool rightIncluded)
            {
                base.Include(left, right, data, leftIncluded, rightIncluded);
            }

            void ICalendarItems.Exclude(Point<TTime> left, Point<TTime> right, TData data)
            {
                base.Exclude(left, right, data);
            }
            void ICalendarItems.Exclude(TTime? left, TTime? right, TData data, bool leftIncluded, bool rightIncluded)
            {
                base.Exclude(left, right, data, leftIncluded, rightIncluded);
            }

        }

        public interface ICalendarItems
        {
            void Include(Point<TTime> left, Point<TTime> right, TData data);
            void Include(TTime? left, TTime? right, TData data, bool leftIncluded = true, bool rightIncluded = true);

            void Exclude(Point<TTime> left, Point<TTime> right, TData data);
            void Exclude(TTime? left, TTime? right, TData data, bool leftIncluded = true, bool rightIncluded = true);
        }

        public class CalendarItem : PointedInterval<TTime, TData>
        {
            public TDuration? Duration => Calendars<TTime, TDuration>.GetOffset?.Invoke(Left.PointValue, Right.PointValue);

            public CalendarItem(Point<TTime> left = null, Point<TTime> right = null, TData data = default(TData)) : base(left, right, data)
            { }

            public CalendarItem Intersect(Point<TTime> left, Point<TTime> right)
            {
                var l = Left <= left ? left : Left;
                var r = Right >= right ? right : Right;
                return new CalendarItem(l, r, Data) { DataToString = DataToString };
            }

            public override string ToString()
            {
                if (!Duration.HasValue)
                    return base.ToString();

                var data = DataToString?.Invoke(Data) ?? (object)Data;
                return $"{Left}, {Right}, Duration = [{Duration}], Data = [{data}]";
            }
        }
        public class Calendar<TPrevData> : ICalendar
        {
            public abstract class CalendarHelper
            {
                public void Define(ICalendarItems container, Calendars<TTime, TDuration, TPrevData>.CalendarItem todefine)
                {
                    var begin = todefine.Left.PointValue.Value;
                    var end = todefine.Right.PointValue.Value;

                    var left = begin;

                    while (left.CompareTo(end) < 0)
                    {
                        left = ProcessInterval(container, left, end, todefine.Data);
                    }
                }

                protected abstract TTime ProcessInterval(ICalendarItems container, TTime left, TTime end, TPrevData data);
                public abstract TData Include(TData a, TData b);
                public abstract TData Exclude(TData a, TData b);

                public virtual ToDebugString ToDebugString => null;
            }

            public delegate void DefineData(ICalendarItems container, Calendars<TTime, TDuration, TPrevData>.CalendarItem toDefine);

            private readonly Calendars<TTime, TDuration, TPrevData>.ICalendar _prev;
            private readonly DefineData _defineData;
            private readonly CalendarItems _calendarItems;
            //private readonly List<IC> _nexts = new List<IC>();

            public Calendar
                (
                DefineData defineData,
                Addition includeData,
                Substraction excludeData,
                ToDebugString dataToString = null
                )
                : this(null, defineData, includeData, excludeData, dataToString)
            {
            }

            public Calendar
                (
                Calendars<TTime, TDuration, TPrevData>.ICalendar prev,
                DefineData defineData,
                Addition includeData,
                Substraction excludeData,
                ToDebugString dataToString = null
                )
            {
                // проверить на закольцовывание... на вс€кий случай....
                if (prev?.FullChain().Contains(this) == true)
                    throw new ArgumentException("try to loop detected", nameof(prev));

                _prev = prev;
                //_prev?._nexts.Add(this);

                _defineData = defineData;
                _calendarItems = new CalendarItems(includeData, excludeData, dataToString);
            }

            public Calendar(CalendarHelper helper)
                : this(helper.Define, helper.Include, helper.Exclude, helper.ToDebugString)
            {
            }

            public Calendar(Calendars<TTime, TDuration, TPrevData>.ICalendar prev, CalendarHelper helper)
                : this(prev, helper.Define, helper.Include, helper.Exclude, helper.ToDebugString)
            {
            }

            public IAbstractCalendar[] FullChain()
            {
                var chain = new IAbstractCalendar[] { this };

                if (_prev != null)
                    chain = _prev.FullChain().Union(chain).ToArray();

                return chain;
            }

            public IEnumerable<CalendarItem> Get(TTime from, TTime to)
            {

                var result = new List<CalendarItem>();

                var left = Point<TTime>.Left(from, false);
                var right = Point<TTime>.Right(to, false);

                var node = _calendarItems.FindNode(x => x.Contains(left));

                while (node != null && node.Value.Left <= right)
                {
                    if (Equals(node.Value.Data, default(TData)))
                    {
                        var prev = node.Previous;
                        var undefinedInterval = node.Value.Intersect(left, right);
                        Define(undefinedInterval);

                        node = (prev ?? _calendarItems.FirstNode).Next;
                        if (Equals(node.Value.Data, default(TData)))
                            node = node.Next;
                    }

                    while (node != null && !Equals(node.Value.Data, default(TData)))
                    {
                        result.Add(node.Value);
                        node = node.Next;
                    }
                }

                return result;
            }

            private void Define(CalendarItem undefinedInterval)
            {
                if (undefinedInterval == null)
                    throw new ArgumentNullException(nameof(undefinedInterval));

                var intervals =
                    _prev?.Get(undefinedInterval.Left.PointValue.Value, undefinedInterval.Right.PointValue.Value)
                    ?? new[] { new Calendars<TTime, TDuration, TPrevData>.CalendarItem(undefinedInterval.Left, undefinedInterval.Right) };



                foreach (var interval in intervals)
                {
                    _defineData(_calendarItems, interval);
                }
            }

            public override string ToString()
            {
                return $"{GetType().TypeName()}";
            }
        }

        public class Calendar : Calendar<TData>
        {
            public new delegate void DefineData(ICalendarItems container, CalendarItem toDefine);

            public new abstract class CalendarHelper : Calendar<TData>.CalendarHelper
            {
            }

            public Calendar(DefineData defineData, Addition includeData, Substraction excludeData, ToDebugString dataToString = null)
                : this
                (
                      null,
                      defineData,
                      includeData,
                      excludeData,
                      dataToString
                      )
            {
            }

            public Calendar(ICalendar prev, DefineData defineData, Addition includeData, Substraction excludeData, ToDebugString dataToString = null)
                : base
                    (
                    prev,
                    (container, define) => defineData(container, define),
                    includeData,
                    excludeData,
                    dataToString
                    )
            {
            }

            public Calendar(CalendarHelper helper) : base(helper)
            {
            }

            public Calendar(ICalendar prev, CalendarHelper helper) : base(prev, helper)
            {
            }
        }

    }
}