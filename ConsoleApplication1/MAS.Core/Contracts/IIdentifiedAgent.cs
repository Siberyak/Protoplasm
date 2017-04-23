using System;

namespace MAS.Core.Contracts
{
    public interface IIdentifiedAgent : IAgent
    {
        Guid ID { get; }
    }
}