using System;
using System.Linq;

namespace Protoplasm.Calendars.Tests
{
    class WorkCalendar : WorkCalendar<int?>
    {
        public int WorkHours(int year)
        {
            return Amount(year) ?? 0;
        }

        protected override int? Amount(int days, int? data)
        {
            return days*(data ?? 0);
        }

        public WorkCalendar(Calendars<DateTime,TimeSpan,int?>.ICalendarItemsAdapter adapter)
            : base(adapter)
        {
        }
    }
}