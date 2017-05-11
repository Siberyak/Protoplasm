using System;
using System.Collections.Generic;
using MAS.Core.Contracts;

namespace MAS.Core.Compatibility.Contracts
{
    public interface IAbilitiesHolder : IHolder
    {
        IReadOnlyCollection<IAbility> Abilities { get; }
        IHoldersCompatibilityInfo Compatible(IRequirementsHolder requirements);
        bool Compatible(IRequirementsHolder requirements, IScene scene);
        IAbility ToScene(IAbility ability);
    }
}