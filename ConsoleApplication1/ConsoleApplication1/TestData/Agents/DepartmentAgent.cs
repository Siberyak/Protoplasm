using System.Collections.Generic;
using MAS.Core;
using MAS.Core.Contracts;
using MAS.Utils;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class DepartmentAgent : ManagedEntityAgent<Department>
        {

            public override void Initialize()
            {
                
            }

            protected override INegotiator Negotiator(IScene scene)
            {
                throw new System.NotImplementedException();
            }

            public DepartmentAgent(ResourcesManager manager, Department entity) : base(manager, entity)
            {
            }
        }
    }
}