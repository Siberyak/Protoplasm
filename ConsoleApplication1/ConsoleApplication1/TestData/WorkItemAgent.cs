namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class WorkItemAgent : EntityAgent<WorkItem>
        {
            public WorkItemAgent(WorkItem entity) : base(entity)
            {
            }

            protected override void RegisterBehaviors()
            {
            
            }
        } 
    }
}