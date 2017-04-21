using System.Collections.Generic;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class DepartmentAgent : EntityAgent<Department>
        {
            public DepartmentAgent(Department entity) : base(entity)
            {
            }

            protected override void RegisterBehaviors()
            {

            }
        }
    }
}