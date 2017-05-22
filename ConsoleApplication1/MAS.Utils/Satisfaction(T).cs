using System;
using MAS.Core.Contracts;

namespace MAS.Utils
{
    interface IFlatableSatisfaction : ISatisfaction
    {
        void Flat();
    }
    public abstract class Satisfaction<T> : ISatisfaction<T>, IFlatableSatisfaction
        where T : struct, IComparable<T>
    {
        protected T _original;

        protected Satisfaction(T original)
        {
            _original = original;
        }

        public T Δ { get; set; }
        public abstract T Value { get; }

        public int CompareTo(ISatisfaction<T> other)
        {
            return other == null ? -1 : Value.CompareTo(other.Value);
        }

        public int CompareTo(ISatisfaction other)
        {
            return CompareTo(other as ISatisfaction<T>);
        }

        public abstract ISatisfaction Snapshot();

        public override string ToString()
        {
            return $"{Value} = {_original} + Δ:[{Δ}]";
        }

        void IFlatableSatisfaction.Flat()
        {
            _original = Value;
            Δ = default(T);
        }
    }
}