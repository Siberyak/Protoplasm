using System.ComponentModel;
using MAS.Core;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        [DisplayName("Employee-Агент")]

        public class EmployeeAgent : ManagedEntityAgent<Employee>, IAbilitiesHolder
        {
            public EmployeeAgent(ResourcesManager manager, Employee entity) : base(manager, entity)
            {
            }


            public override void Initialize()
            {
                
            }

            protected override INegotiator Negotiator(Scene scene)
            {
                return new EmployeeAgentNegotiator(scene, this);
            }
        }

        public class EmployeeAgentNegotiator : Negotiator<EmployeeAgent>
        {
            public EmployeeAgentNegotiator(IScene scene, EmployeeAgent agent) : base(scene, agent)
            {
            }

            public override IScene Variate(INegotiator abilities)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}