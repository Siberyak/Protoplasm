using System;

namespace Protoplasm.Calendars.Tests
{
    abstract class WorkCalendarHelper : Calendars<DateTime, TimeSpan, int?>.Calendar.CalendarHelper
    {
        public override int? Include(int? a, int? b)
        {
            return a.HasValue || b.HasValue
                ? (a ?? 0) + (b ?? 0)
                : default(int?);
        }

        public override int? Exclude(int? a, int? b)
        {
            return a.HasValue || b.HasValue
                ? (a ?? 0) - (b ?? 0)
                : default(int?);
        }
    }
}