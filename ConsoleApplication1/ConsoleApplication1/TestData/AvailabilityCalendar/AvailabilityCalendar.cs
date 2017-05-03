using Protoplasm.Calendars;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {

        public abstract class AvailabilityCalendar : Calendars<TTime, TDuration, IAvailabilityData>.Calendar, IAvailabilityCalendar
        {
            protected AvailabilityCalendar(Calendars<TTime, TDuration, IAvailabilityData>.ICalendarAdapter adapter) : base(adapter)
            {
            }
        }
    }
}