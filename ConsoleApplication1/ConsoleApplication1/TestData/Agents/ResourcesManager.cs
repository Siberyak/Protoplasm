namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class ResourcesManager : BaseAgent
        {
            protected override void RegisterBehaviors()
            {
            
            }

            public void Add(string caption, Competences competences, Calendar<CalendarItemType> calendar, params MembershipItemsContainer[] memberOf)
            {
                Employee employee = new Employee(caption, competences, calendar);
            }
        } 
    }
}