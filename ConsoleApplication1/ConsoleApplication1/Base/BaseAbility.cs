using System;
using ConsoleApplication1.TestData;

namespace ConsoleApplication1
{
    public abstract class BaseAbility
    {
        public virtual bool IsMutable => false;

        public abstract CompatibilityType Compatible(BaseRequirement requirement);
    }
}