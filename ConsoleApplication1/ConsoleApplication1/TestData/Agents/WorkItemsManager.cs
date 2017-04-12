using System.Collections.Generic;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>

    {
        public class WorkItemsManager : BaseAgent
        {
            protected override void RegisterBehaviors()
            {

            }

            public void Add(Interval<TTime?> start, Interval<TTime?> finish, Interval<TDuration?> duration, IReadOnlyCollection<Competence> competences)
            {
                var workItem = new WorkItem(start, finish, duration, competences);
                var agent = new WorkItemAgent(workItem);
                agent.Initialize();
            }
        }
    }
}