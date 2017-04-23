namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        public class Calendar : Calendar<TData>
        {
            public new delegate void DefineData(ICalendarItems container, CalendarItem toDefine);

            public new abstract class CalendarHelper : Calendar<TData>.CalendarHelper
            {
            }

            public Calendar(DefineData defineData, Addition includeData, Substraction excludeData, ToDebugString dataToString = null)
                : this
                    (
                    null,
                    defineData,
                    includeData,
                    excludeData,
                    dataToString
                    )
            {
            }

            public Calendar(ICalendar prev, DefineData defineData, Addition includeData, Substraction excludeData, ToDebugString dataToString = null)
                : base
                    (
                    prev,
                    (container, define) => defineData(container, define),
                    includeData,
                    excludeData,
                    dataToString
                    )
            {
            }

            public Calendar(CalendarHelper helper) : base(helper)
            {
            }

            public Calendar(ICalendar prev, CalendarHelper helper) : base(prev, helper)
            {
            }
        }

    }
}