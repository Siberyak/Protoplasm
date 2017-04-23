using System;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;


namespace MAS.Core
{
    public abstract class BaseAbility : IAbility
    {
        public virtual bool IsMutable => false;

        public abstract CompatibilityType Compatible(IRequirement requirement);
    }
}