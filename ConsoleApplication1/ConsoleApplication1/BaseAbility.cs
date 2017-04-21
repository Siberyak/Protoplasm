using System;
using ConsoleApplication1.TestData;

namespace ConsoleApplication1
{
    public abstract class BaseAbility
    {
        public virtual bool IsMutable => false;

        public abstract ConformType Conformable(BaseRequirement requirement);
    }

    public enum ConformType
    {
        Conform = 0, Posible=1, Imposible=2
    }

    public struct ConformResult
    {
        public static readonly ConformType Empty = new ConformType();

        public bool Handled;
        public bool Conformable;
        public bool IsStatic;
    }
}