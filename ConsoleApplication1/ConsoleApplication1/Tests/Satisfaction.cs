using System;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
{
    public class Satisfaction : Satisfaction<double>
    {
        public Satisfaction(double original) : base(original)
        {
        }

        public override double Value => _original + Delta;
    }

    public abstract class Satisfaction<T> : ISatisfaction<T>
        where T : IComparable<T>
    {
        protected T _original;

        protected Satisfaction(T original)
        {
            _original = original;
        }

        public T Delta { get; set; }
        public abstract T Value { get; }

        public int CompareTo(ISatisfaction<T> other)
        {
            return other == null ? -1 : Value.CompareTo(other.Value);
        }

        public int CompareTo(ISatisfaction other)
        {
            return CompareTo(other as ISatisfaction<T>);
        }
    }
}