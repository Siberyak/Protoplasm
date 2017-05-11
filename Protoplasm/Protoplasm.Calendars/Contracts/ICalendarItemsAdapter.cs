using System;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        public interface ICalendarItemsAdapter //ICalendarItemsAdapter
        {
            TData Include(TData a, TData b);
            TData Exclude(TData a, TData b);

            Func<TData, string> ToDebugString { get; }
        }
    }
}