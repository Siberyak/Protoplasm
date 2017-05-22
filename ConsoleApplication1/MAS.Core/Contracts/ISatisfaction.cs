using System;

namespace MAS.Core.Contracts
{
    public interface ISatisfaction : IComparable<ISatisfaction>
    {
        ISatisfaction Snapshot();
    }
}