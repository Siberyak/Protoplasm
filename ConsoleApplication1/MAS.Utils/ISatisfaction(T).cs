using System;
using MAS.Core.Contracts;

namespace MAS.Utils
{
    public interface ISatisfaction<T> : ISatisfaction, IComparable<ISatisfaction<T>>
        where T : struct, IComparable<T>
    {
        T Δ { get; }
        T Value { get; }
    }
}