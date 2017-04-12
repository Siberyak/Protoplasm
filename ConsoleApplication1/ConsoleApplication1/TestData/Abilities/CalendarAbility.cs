namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class CalendarAbility : Ability
        {
            public Calendar Calendar { get; }

            public CalendarAbility(Calendar calendar) : base(MappingType.Calendar)
            {
                Calendar = calendar;
            }
        } 
    }
}