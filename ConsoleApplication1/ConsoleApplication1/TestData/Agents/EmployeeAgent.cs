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
        [DisplayName("Employee-�����")]

        public class EmployeeAgent : ManagedEntityAgent<Employee>, IAbilitiesHolder
        {
            public EmployeeAgent(ResourcesManager manager, Employee entity) : base(manager, entity)
            {
            }



            public override void Initialize()
            {
                
            }

            protected override INegotiator Negotiator(IScene scene)
            {
                return new EmployeeAgentNegotiator(scene, this);
            }
        }

        public class EmployeeAgentNegotiator : Negotiator<EmployeeAgent>
        {
            public EmployeeAgentNegotiator(IScene scene, EmployeeAgent agent) : base(scene, agent)
            {
            }

            //public override IScene Variate(INegotiator respondent)
            //{
            //    throw new System.NotImplementedException();
            //}
            public override IEnumerable<IScene> Variants(INegotiator respondent)
            {
                throw new System.NotImplementedException();
            }

            protected override void MergeToOriginal(INegotiator original)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}