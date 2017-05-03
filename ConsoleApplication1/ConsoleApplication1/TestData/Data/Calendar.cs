using System;
using Protoplasm.Calendars;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {

        public Calendars<TTime, TDuration, TData>.Calendar CreateCalendar<TData>(Calendars<TTime,TDuration,TData>.CalendarAdapter adapter)
        {
            return new Calendars<TTime, TDuration, TData>.Calendar(adapter);
        }
    }
}