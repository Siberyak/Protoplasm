using System.Collections.Generic;

namespace MAS.Core
{
    public abstract class BaseRequirement
    {
        public static IReadOnlyCollection<BaseRequirement> Empty = new BaseRequirement[0];
        public virtual bool IsMutable => true;
        public abstract CompatibilityType Compatible(BaseAbility ability);
    }
}