using MAS.Core;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class CalendarAbility : Ability
        {
            public Calendar<TestCalendarItemType> Calendar { get; }

            public CalendarAbility(Calendar<TestCalendarItemType> calendar) : base(MappingType.Calendar)
            {
                Calendar = calendar;
            }

            public override CompatibilityType Compatible(BaseRequirement requirement)
            {
                return CompatibilityType.DependsOnScene;
            }
        } 
    }
}