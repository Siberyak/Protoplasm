using System;
using MAS.Core.Compatibility.Contracts;

namespace MAS.Core.Contracts
{
    public interface IAgent : IEquatable<IAgent>, IRespondent
    {
        void Initialize();
    }
}