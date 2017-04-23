namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class Calendar<TData> : Calendars<TTime, TDuration, TData>.Calendar
        {
            public Calendar(DefineData defineData, Calendars<TTime, TDuration, TData>.Addition includeData, Calendars<TTime, TDuration, TData>.Substraction excludeData, Calendars<TTime, TDuration, TData>.ToDebugString dataToString = null) : base(defineData, includeData, excludeData, dataToString)
            {
            }

            public Calendar(Calendars<TTime, TDuration, TData>.ICalendar prev, DefineData defineData, Calendars<TTime, TDuration, TData>.Addition includeData, Calendars<TTime, TDuration, TData>.Substraction excludeData, Calendars<TTime, TDuration, TData>.ToDebugString dataToString = null) : base(prev, defineData, includeData, excludeData, dataToString)
            {
            }

            public Calendar(CalendarHelper helper) : base(helper)
            {
            }

            public Calendar(Calendars<TTime, TDuration, TData>.ICalendar prev, CalendarHelper helper) : base(prev, helper)
            {
            }
        }

        public Calendar<TData> CreateCalendar<TData>
            (
            Calendars<TTime, TDuration, TData>.Calendar.DefineData defineData,
            Calendars<TTime, TDuration, TData>.Addition includeData,
            Calendars<TTime, TDuration, TData>.Substraction excludeData,
            Calendars<TTime, TDuration, TData>.ToDebugString dataToString = null
            )
        {
            return new Calendar<TData>(defineData, includeData, excludeData, dataToString);
        }

        public Calendar<TData> CreateCalendar<TData>(Calendars<TTime, TDuration, TData>.Calendar.CalendarHelper helper)
        {
            return new Calendar<TData>(helper);
        }
    }

}