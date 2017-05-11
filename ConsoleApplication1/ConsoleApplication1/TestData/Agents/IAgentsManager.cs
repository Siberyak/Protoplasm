using System.Collections.Generic;
using MAS.Core.Compatibility.Contracts;

namespace ConsoleApplication1.TestData
{
    public interface IAgentsManager
    {
        IHoldersCompatibilityInfo Compatible(IAbilitiesHolder abilities, IRequirementsHolder requirements);
        IHoldersCompatibilityInfo Find(IRequirementsHolder requirements, IAbilitiesHolder abilities);
        IHoldersCompatibilityInfo Add(IRequirementsHolder requirements, IAbilitiesHolder abilities, IHoldersCompatibilityInfo info);
        IEnumerable<IHoldersCompatibilityInfo> this[IRequirementsHolder requirements] { get; }
    }
}