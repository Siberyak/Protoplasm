using System;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Utils
{
    public abstract class Requirement : IRequirement
    {
        public virtual bool IsMutable => false;
        public abstract CompatibilityType Compatible(IAbility ability);

        public abstract bool Compatible(IAbility ability, IScene scene);

        public virtual IRequirement ToScene()
        {
            throw new NotImplementedException();
        }
    }
}