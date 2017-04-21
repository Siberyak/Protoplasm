using System.Collections.Generic;

namespace ConsoleApplication1
{
    public abstract class BaseRequirement
    {
        public static IReadOnlyCollection<BaseRequirement> Empty = new BaseRequirement[0];
        public virtual bool IsMutable => true;
        public abstract ConformType Conformable(BaseAbility ability);
    }
}