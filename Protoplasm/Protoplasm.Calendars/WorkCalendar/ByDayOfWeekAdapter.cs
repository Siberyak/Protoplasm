using System;

namespace Protoplasm.Calendars
{
    public abstract class ByDayOfWeekAdapter<T> : Calendars<DateTime, TimeSpan, T>.CalendarAdapter<T>
    {
        protected readonly T DaylyWorkData;
        protected readonly T DaylyNotWorkData;

        protected ByDayOfWeekAdapter(T daylyWorkData, T daylyNotWorkData, Calendars<DateTime, TimeSpan, T>.ICalendar baseCalendar) : base(baseCalendar)
        {
            DaylyWorkData = daylyWorkData;
            DaylyNotWorkData = daylyNotWorkData;
        }

        protected override DateTime DefineByBaseData(Calendars<DateTime, TimeSpan, T>.ICalendarItems container, DateTime left, DateTime end, T baseData)
        {
            DateTime? right = left;
            var value = DaylyNotWorkData;
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
                    value = DaylyWorkData;
                    break;
            }

            right = Calendars<DateTime, TimeSpan>.Min(right.Value, end).AddDays(1);

            container.Include(left, right, value, rightIncluded: false);

            return right.Value;
        }
    }
}