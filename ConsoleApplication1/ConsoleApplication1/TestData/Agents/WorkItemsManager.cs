using System.Collections.Generic;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>

    {
        public class WorkItemsManager : BaseAgent
        {
            private PlanningEnvironment<TTime, TDuration> _environment;

            public WorkItemsManager(PlanningEnvironment<TTime, TDuration> environment)
            {
                _environment = environment;
            }

            protected override void RegisterBehaviors()
            {

            }

            public WorkItemAgent CreateWorkItemAgent(Interval<TTime?> start, Interval<TTime?> finish, Interval<TDuration?> duration, IReadOnlyCollection<Competence> competences)
            {
                var workItem = new WorkItem(start, finish, duration, competences);
                var agent = new WorkItemAgent(workItem);
                agent.Initialize();
                return agent;
            }
        }
    }
}