using System;
using Protoplasm.PointedIntervals;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {

        public abstract class CalendarAdapter : ICalendarAdapter
        {
            public IAbstractCalendar BaseCalendar => null;

            public virtual void Define(ICalendarItems container, Point<TTime> left, Point<TTime> right)
            {
                var begin = left.PointValue.Value;
                var end = right.PointValue.Value;

                var current = begin;

                while (current.CompareTo(end) < 0)
                {
                    current = Define(container, current, end);
                }
            }

            protected abstract TTime Define(ICalendarItems container, TTime left, TTime right);

            public abstract TData Include(TData a, TData b);
            public abstract TData Exclude(TData a, TData b);

            public virtual Func<TData, string> ToDebugString => null;
        }

        public abstract class CalendarAdapter<TBaseData> : ICalendarAdapter
        {
            public IAbstractCalendar BaseCalendar => _baseCalendar;

            protected readonly Calendars<TTime, TDuration, TBaseData>.ICalendar _baseCalendar;

            protected CalendarAdapter(Calendars<TTime, TDuration, TBaseData>.ICalendar baseCalendar)
            {
                _baseCalendar = baseCalendar;
            }

            public virtual void Define(ICalendarItems container, Point<TTime> left, Point<TTime> right)
            {
                var intervals =
                    _baseCalendar?.Get(left, right)
                    ?? new[] {new Calendars<TTime, TDuration, TBaseData>.CalendarItem(left, right)};

                foreach (var interval in intervals)
                {
                    Define(container, interval);
                }

            }

            protected virtual void Define(ICalendarItems container, Calendars<TTime, TDuration, TBaseData>.CalendarItem todefine)
            {
                var right = todefine.Right;
                var left = todefine.Left;

                while (left < right)
                {
                    left = DefineByBaseData(container, left, right, todefine.Data);
                }
            }

            protected abstract Point<TTime> DefineByBaseData(ICalendarItems container, Point<TTime> left, Point<TTime> right, TBaseData baseData);
            public abstract TData Include(TData a, TData b);
            public abstract TData Exclude(TData a, TData b);

            public virtual Func<TData, string> ToDebugString => null;
        }
    }
}