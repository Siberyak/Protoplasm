namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class CalendarAbility : Ability
        {
            public Calendar<CalendarItemType> Calendar { get; }

            public CalendarAbility(Calendar<CalendarItemType> calendar) : base(MappingType.Calendar)
            {
                Calendar = calendar;
            }

            public override ConformType Conformable(BaseRequirement requirement)
            {
                return ConformType.Posible;
            }
        } 
    }
}