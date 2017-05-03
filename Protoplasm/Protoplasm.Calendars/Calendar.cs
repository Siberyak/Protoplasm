using System;
using System.Collections.Generic;
using System.Linq;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        public interface ICalendarAdapter
        {
            IAbstractCalendar BaseCalendar { get; }

            void Define(ICalendarItems container, Point<TTime> left, Point<TTime> right);
            
            TData Include(TData a, TData b);
            TData Exclude(TData a, TData b);

            Func<TData, string> ToDebugString { get; }
        }
        public abstract class CalendarAdapter : ICalendarAdapter
        {
            public IAbstractCalendar BaseCalendar => null;

            public void Define(ICalendarItems container, Point<TTime> left, Point<TTime> right)
            {
                var begin = left.PointValue.Value;
                var end = right.PointValue.Value;

                var current = begin;

                while (current.CompareTo(end) < 0)
                {
                    current = Define(container, current, end);
                }
            }

            protected abstract TTime Define(ICalendarItems container, TTime current, TTime end);

            public abstract TData Include(TData a, TData b);
            public abstract TData Exclude(TData a, TData b);

            public virtual Func<TData, string> ToDebugString => null;
        }
        public abstract class CalendarAdapter<TBaseData> : ICalendarAdapter
        {
            public IAbstractCalendar BaseCalendar => _baseCalendar;

            private readonly Calendars<TTime, TDuration, TBaseData>.ICalendar _baseCalendar;

            protected CalendarAdapter(Calendars<TTime, TDuration, TBaseData>.ICalendar baseCalendar)
            {
                _baseCalendar = baseCalendar;
            }

            public void Define(ICalendarItems container, Point<TTime> left, Point<TTime> right)
            {
                var intervals =
                    _baseCalendar?.Get(left.PointValue.Value, right.PointValue.Value)
                    ?? new[] {new Calendars<TTime, TDuration, TBaseData>.CalendarItem(left, right)};

                foreach (var interval in intervals)
                {
                    Define(container, interval);
                }

            }

            private void Define(ICalendarItems container, Calendars<TTime, TDuration, TBaseData>.CalendarItem todefine)
            {
                var begin = todefine.Left.PointValue.Value;
                var end = todefine.Right.PointValue.Value;

                var left = begin;

                while (left.CompareTo(end) < 0)
                {
                    left = DefineByBaseData(container, left, end, todefine.Data);
                }
            }

            protected abstract TTime DefineByBaseData(ICalendarItems container, TTime left, TTime end, TBaseData baseData);
            public abstract TData Include(TData a, TData b);
            public abstract TData Exclude(TData a, TData b);

            public virtual Func<TData, string> ToDebugString => null;
        }

        public class Calendar : ICalendar
        {
            protected readonly ICalendarAdapter _adapter;
            private readonly CalendarItems _calendarItems;

            public Calendar(ICalendarAdapter adapter)
            {
                _adapter = adapter;

                // проверить на закольцовывание... на вс€кий случай....
                if (_adapter.BaseCalendar?.FullChain().Contains(this) == true)
                    throw new ArgumentException("try to loop detected", nameof(_adapter));

                _calendarItems = new CalendarItems(_adapter.Include, _adapter.Exclude, _adapter.ToDebugString);
            }

            public IAbstractCalendar[] FullChain()
            {
                var chain = new IAbstractCalendar[] { this };

                if (_adapter.BaseCalendar != null)
                    chain = _adapter.BaseCalendar.FullChain().Union(chain).ToArray();

                return chain;
            }

            public IEnumerable<CalendarItem> Get(TTime from, TTime to)
            {
                var left = Point<TTime>.Left(from, false);
                var right = Point<TTime>.Right(to, false);

                var node = _calendarItems.FindNode(x => x.Contains(left));

                while (node != null && node.Value.Left <= right)
                {
                    if (Equals(node.Value.Data, default(TData)))
                    {
                        var prev = node.Previous;
                        var prevprev = prev?.Previous;

                        var undefinedInterval = node.Value.Intersect(left, right);
                        Define(undefinedInterval);

                        if (prev?.Alive != true)
                            prev = prevprev;

                        node = (prev ?? _calendarItems.FirstNode).Next;
                        if (Equals(node.Value.Data, default(TData)))
                            node = node.Next;
                    }

                    node = node?.Next;
                }

                var result = _calendarItems.SkipWhile(x => !x.Contains(left)).TakeWhile(x => x.Left < right).ToList();

                var first = result.FirstOrDefault();
                if (first != null && first.Left != left)
                {
                    result[0] = new CalendarItem(left, first.Right, first.Data);
                }

                var last = result.LastOrDefault();
                if (last != null && last.Right != right)
                {
                    result[result.Count - 1] = new CalendarItem(last.Left, right, last.Data);
                }

                return result;
            }

            private void Define(CalendarItem undefinedInterval)
            {
                if (undefinedInterval == null)
                    throw new ArgumentNullException(nameof(undefinedInterval));

                _adapter.Define(_calendarItems, undefinedInterval.Left, undefinedInterval.Right);
            }

            public override string ToString()
            {
                return $"{GetType().TypeName()}";
            }
        }


    }
}