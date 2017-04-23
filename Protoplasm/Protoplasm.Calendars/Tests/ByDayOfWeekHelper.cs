using System;

namespace Protoplasm.Calendars.Tests
{
    internal class ByDayOfWeekHelper : WorkCalendarHelper
    {
        protected override DateTime ProcessInterval(Calendars<DateTime, TimeSpan, int?>.ICalendarItems container, DateTime left, DateTime end, int? data)
        {
            DateTime? right = left;
            var value = 0;
            var dayOfWeek = left.DayOfWeek;
            switch (dayOfWeek)
            {
                case DayOfWeek.Saturday:
                    right = left.AddDays(1);
                    break;
                case DayOfWeek.Sunday:
                    break;
                default:
                    right = left.AddDays(5 - (int)dayOfWeek);
                    value = 8;
                    break;
            }

            right = Calendars<DateTime, TimeSpan>.Min(right.Value, end).AddDays(1);

            container.Include(left, right, value, rightIncluded: false);

            return right.Value;
        }
    }
}