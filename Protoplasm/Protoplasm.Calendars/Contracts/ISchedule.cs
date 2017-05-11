using System;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData> 
    {
        public interface ISchedule : ICalendarItems
        {
            ICalendarItemsAdapter Adapter { get; }
            ICalendar Available { get; }
            ICalendarItemsProvider Allocated { get; }

            ISchedule Clone();

            TDuration MinDuration { get; }
        }
    }
}