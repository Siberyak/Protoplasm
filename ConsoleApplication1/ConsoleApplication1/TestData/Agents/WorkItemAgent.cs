using System.Collections.Generic;
using System.ComponentModel;
using MAS.Core;
using MAS.Core.Compatibility.Contracts;

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
        }
    }
}