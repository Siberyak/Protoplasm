using System;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
{
    public interface ISatisfaction<T> : ISatisfaction, IComparable<ISatisfaction<T>>
        where T : IComparable<T>
    {
        T Delta { get; }
        T Value { get; }
    }
}