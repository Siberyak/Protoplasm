using System;
using Protoplasm.PointedIntervals;

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

        protected override Point<DateTime> DefineByBaseData(Calendars<DateTime, TimeSpan, T>.ICalendarItems container, Point<DateTime> left, Point<DateTime> right, T baseData)
        {

            left = left.AsDate(true);
            right = right.AsDate(false);

            var end = right;

            right = left.AsRight().AddDays(1);
            var value = DaylyNotWorkData;
            var dayOfWeek = left.DayOfWeek();

            switch (dayOfWeek)
            {
                case DayOfWeek.Saturday:
                    right = right.AddDays(1);
                    break;
                case DayOfWeek.Sunday:
                    break;
                default:
                    right = right.AddDays(5 - (int)dayOfWeek);
                    value = DaylyWorkData;
                    break;
            }

            right = Point<DateTime>.Min(right, end);

            container.Include(left, right, value);

            return right.AsLeft();
        }
    }
}