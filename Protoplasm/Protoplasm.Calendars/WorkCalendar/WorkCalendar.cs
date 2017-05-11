using System;
using System.Linq;

namespace Protoplasm.Calendars
{
    public abstract class WorkCalendar<T> : Calendars<DateTime, TimeSpan, T>.Calendar
    {
        public T Amount(int year)
        {
            var left = new DateTime(year, 1, 1);
            var items = Get(left.Left(true), left.AddYears(1).Right(false));
            var result = items.Select(Amount).Aggregate((a, b) => Adapter.Include(a, b));
            return result;
        }

        private T Amount(Calendars<DateTime, TimeSpan, T>.CalendarItem x)
        {
            var duration = x.Duration ?? x.Right.PointValue - x.Left.PointValue ?? TimeSpan.Zero;
            var days = duration.Days;

            return Amount(days, x.Data);
        }

        protected abstract T Amount(int days, T data);


        protected WorkCalendar(Calendars<DateTime, TimeSpan, T>.ICalendarItemsAdapter adapter)
            : base(adapter)
        {
        }
    }
}