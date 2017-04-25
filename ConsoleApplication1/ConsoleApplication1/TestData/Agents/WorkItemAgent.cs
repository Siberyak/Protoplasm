using System.Collections.Generic;
using MAS.Core;
using MAS.Core.Compatibility.Contracts;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class WorkItemAgent : ManagedEntityAgent<WorkItem>, IRequirementsHolder
        {
            public WorkItemAgent(WorkItemsManager manager, WorkItem entity) : base(manager, entity)
            {
            }


            public bool Equals(IRequirementsHolder other)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}