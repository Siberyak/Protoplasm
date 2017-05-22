using System.Collections.Generic;
using System.ComponentModel;
using MAS.Core;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using MAS.Utils;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        [DisplayName("WorkItem-�����")]
        public class WorkItemAgent : ManagedEntityAgent<WorkItem>
        {
            public WorkItemAgent(WorkItemsManager manager, WorkItem entity) : base(manager, entity)
            {
            }



            public override void Initialize()
            {
                
            }

            protected override INegotiator Negotiator(IScene scene)
            {
                return new WorkItemNegotiator(scene, this);
            }
        }

        class WorkItemNegotiator : Negotiator<WorkItemAgent>
        {
            public override bool IsSatisfied => Satisfaction.Value > 0;

            public WorkItemNegotiator(IScene scene, WorkItemAgent agent) : base(scene, agent)
            {
            }

            public override IScene Variate(INegotiator respondent)
            {
                return VariateByEmployee(respondent as EmployeeAgentNegotiator);
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