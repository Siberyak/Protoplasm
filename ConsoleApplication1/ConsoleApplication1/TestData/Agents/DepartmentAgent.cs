using System.Collections.Generic;
using MAS.Core;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class DepartmentAgent : ManagedEntityAgent<Department>
        {
            public DepartmentAgent(ResourcesManager manager, Department entity) : base(manager, entity)
            {
            }

            protected override void RegisterBehaviors()
            {

            }
        }
    }
}