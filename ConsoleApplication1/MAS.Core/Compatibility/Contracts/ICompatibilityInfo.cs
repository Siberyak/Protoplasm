using System.Collections.Generic;
using MAS.Core.Contracts;

namespace MAS.Core.Compatibility.Contracts
{
    public interface ICompatibilityInfo
    {
        IRequirement Requirement { get; }
        IReadOnlyDictionary<CompatibilityType, IAbility[]> Compatibility { get; }

        bool Compatible(IScene scene);
    }
}