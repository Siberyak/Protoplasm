using System;
using System.Collections.Generic;
using Protoplasm.PointedIntervals;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        public interface ICalendarItemsProvider
        {
            IEnumerable<CalendarItem> Get(TTime left, TTime right, bool leftIncluded = false, bool rightIcluded = false);
            IEnumerable<CalendarItem> Get(Point<TTime> from, Point<TTime> to);
            CalendarItem[] DefinedItems();

            INode<ICalendarItem> Find(Point<TTime> point);
        }

        public interface ICalendar : IAbstractCalendar, ICalendarItemsProvider
        {
            ICalendarItemsAdapter Adapter { get; }
            ICalendarAdapter CalendarAdapter { get; }

            ISchedule CreateSchedule(TDuration epsilon);


        }
    }
}