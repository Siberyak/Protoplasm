using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using Protoplasm.Calendars;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class ScheduleAbility : Ability
        {
            private readonly CalendarAbility _calendarAbility;

            public ScheduleAbility(CalendarAbility calendarAbility)
            {
                _calendarAbility = calendarAbility;
            }

            public override bool Compatible(IRequirement requirement, IScene scene)
            {
                //_calendarAbility.Calendar.CreateSchedule();
                //new Schedule<WortTypedCalendarItem>(_calendarAbility.Calendar, new AllocationsHelper());
                return true;
                //_calendarAbility.Calendar
                //scene.GetData<Schedule<>>()
            }

            public override CompatibilityType Compatible(IRequirement requirement)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}