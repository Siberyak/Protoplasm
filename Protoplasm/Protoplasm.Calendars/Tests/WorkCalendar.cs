using System;
using System.Linq;

namespace Protoplasm.Calendars.Tests
{
    class WorkCalendar : Calendars<DateTime, TimeSpan, int?>.Calendar
    {

        public int WorkHours(int year)
        {
            var left = new DateTime(year, 1, 1);
            var items = Get(left, left.AddYears(1).AddDays(-1));

            var hours = items.Sum(Hours);

            return hours;
        }

        private static int Hours(Calendars<DateTime, TimeSpan, int?>.CalendarItem x)
        {
            var duration = x.Duration ?? (x.Right.PointValue - x.Left.PointValue) ?? TimeSpan.Zero;
            return duration.Days * (x.Data ?? 0);
        }


        public WorkCalendar(WorkCalendarHelper helper)
            : base(helper)
        {
        }

        public WorkCalendar(WorkCalendar prev, WorkCalendarHelper helper)
            : base(prev, helper)
        {
        }
    }
}