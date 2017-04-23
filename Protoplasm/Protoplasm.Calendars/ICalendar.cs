using System.Collections.Generic;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        public interface ICalendar : IAbstractCalendar
        {
            IEnumerable<CalendarItem> Get(TTime from, TTime to);
        }
    }
}