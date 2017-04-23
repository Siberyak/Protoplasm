using MAS.Core;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    { 
        public class EmployeeAgent : EntityAgent<Employee>
        {
            public EmployeeAgent(Employee entity) : base(entity)
            {
            }

            protected override void RegisterBehaviors()
            {
                
            }
        }
    }
}