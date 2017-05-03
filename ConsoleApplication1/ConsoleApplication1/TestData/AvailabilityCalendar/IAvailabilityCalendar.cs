using Protoplasm.Calendars;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public interface IAvailabilityCalendar : Calendars<TTime, TDuration, IAvailabilityData>.ICalendar
        {
        }
    }
}