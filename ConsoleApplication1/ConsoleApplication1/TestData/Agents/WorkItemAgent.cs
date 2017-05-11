using System.Collections.Generic;
using System.ComponentModel;
using MAS.Core;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        [DisplayName("WorkItem-Агент")]
        public class WorkItemAgent : ManagedEntityAgent<WorkItem>
        {
            public WorkItemAgent(WorkItemsManager manager, WorkItem entity) : base(manager, entity)
            {
            }


            public override void Initialize()
            {
                
            }

            protected override INegotiator Negotiator(Scene scene)
            {
                return new WorkItemNegotiator(scene, this);
            }
        }

        class WorkItemNegotiator : Negotiator<WorkItemAgent>
        {
            public WorkItemNegotiator(IScene scene, WorkItemAgent agent) : base(scene, agent)
            {
            }

            public override IScene Variate(INegotiator abilities)
            {
                return VariateByEmployee(abilities as EmployeeAgentNegotiator);
            }

            private IScene VariateByEmployee(EmployeeAgentNegotiator negotiator)
            {
                if (negotiator == null)
                    return null;

                //negotiator.Scheduler

                return null;
            }
        }
    }
}