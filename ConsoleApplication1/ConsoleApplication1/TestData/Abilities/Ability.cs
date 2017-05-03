using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MAS.Core;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
{
    public abstract class Ability : IAbility
    {
        public virtual bool IsMutable => false;

        public abstract bool Compatible(IRequirement requirement, IScene scene);

        public abstract CompatibilityType Compatible(IRequirement requirement);

        public virtual IAbility ToScene()
        {
            throw new NotImplementedException();
        }
    }
}
