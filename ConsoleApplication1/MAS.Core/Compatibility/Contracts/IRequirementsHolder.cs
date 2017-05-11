using System;
using System.Collections.Generic;
using MAS.Core.Contracts;

namespace MAS.Core.Compatibility.Contracts
{
    public interface IRequirementsHolder : IHolder
    {
        IReadOnlyCollection<IRequirement> Requirements { get; }
        IHoldersCompatibilityInfo Compatible(IAbilitiesHolder abilities);
        bool Compatible(IAbilitiesHolder abilities, IScene scene);
        IRequirement ToScene(IRequirement requirement);
    }
}