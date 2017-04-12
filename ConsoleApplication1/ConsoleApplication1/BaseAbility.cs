using System;
using ConsoleApplication1.TestData;

namespace ConsoleApplication1
{
    public abstract class BaseAbility
    {
        protected abstract ConformResult Conformable(BaseRequirement requirement);
    }

    public struct ConformResult
    {
        public static readonly ConformResult Empty = new ConformResult();

        public bool Handled;
        public bool Conformable;
        public bool IsStatic;
    }
}