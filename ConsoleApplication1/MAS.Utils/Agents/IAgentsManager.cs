using System.Collections.Generic;
using MAS.Core.Compatibility.Contracts;

namespace MAS.Utils
{
    public interface IAgentsManager : IEnumerable<IHoldersCompatibilityInfo>
    {
        IEnumerable<IHoldersCompatibilityInfo> RegisterCompatibility(IAbilitiesHolder abilities);
        IEnumerable<IHoldersCompatibilityInfo> RegisterCompatibility(IRequirementsHolder requirements);

        IHoldersCompatibilityInfo Compatible(IAbilitiesHolder abilities, IRequirementsHolder requirements);
        IHoldersCompatibilityInfo Find(IRequirementsHolder requirements, IAbilitiesHolder abilities);
        IHoldersCompatibilityInfo Add(IRequirementsHolder requirements, IAbilitiesHolder abilities, IHoldersCompatibilityInfo info);
        IEnumerable<IHoldersCompatibilityInfo> this[IRequirementsHolder requirements] { get; }
    }
}