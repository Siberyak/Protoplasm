using Protoplasm.Calendars;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {

        public abstract class AvailabilityCalendarAdapter : Calendars<TTime,TDuration,IAvailabilityData>.CalendarAdapter<IAvailabilityData>
        {
            protected AvailabilityCalendarAdapter(IAvailabilityCalendar baseCalendar) : base(baseCalendar)
            {}

            public override IAvailabilityData Include(IAvailabilityData a, IAvailabilityData b)
            {
                return a.Include(b);
            }

            public override IAvailabilityData Exclude(IAvailabilityData a, IAvailabilityData b)
            {
                return a.Exclude(b);
            }
        }
    }
}