using System.Collections.Generic;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Core
{
    public abstract class BaseRequirement : IRequirement
    {
        public static IReadOnlyCollection<BaseRequirement> Empty = new BaseRequirement[0];
        public virtual bool IsMutable => true;
        public abstract CompatibilityType Compatible(IAbility ability);
    }
}