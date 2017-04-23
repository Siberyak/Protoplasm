using System;


namespace MAS.Core
{
    public abstract class BaseAbility
    {
        public virtual bool IsMutable => false;

        public abstract CompatibilityType Compatible(BaseRequirement requirement);
    }
}