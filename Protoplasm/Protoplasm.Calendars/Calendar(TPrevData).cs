using System;
using System.Collections.Generic;
using System.Linq;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
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
                var chain = new IAbstractCalendar[] {this};

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
                    ?? new[] {new Calendars<TTime, TDuration, TPrevData>.CalendarItem(undefinedInterval.Left, undefinedInterval.Right)};



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
    }
}