using System.ComponentModel;
using MAS.Core;
using MAS.Core.Compatibility.Contracts;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        [DisplayName("Employee-�����")]

        public class EmployeeAgent : ManagedEntityAgent<Employee>, IAbilitiesHolder
        {
            public EmployeeAgent(ResourcesManager manager, Employee entity) : base(manager, entity)
            {
            }

            public bool Equals(IAbilitiesHolder other)
            {
                throw new System.NotImplementedException();
            }

            

        }
    }
}