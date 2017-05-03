using System.Collections.Generic;
using MAS.Core.Contracts;

namespace MAS.Core.Compatibility.Contracts
{
    public interface IHoldersCompatibilityInfo 
    {
        IRequirementsHolder RequiremenetsHolder { get; }
        IAbilitiesHolder AbilitiesHolder { get; }
        CompatibilityType Compatibility { get; }
        IReadOnlyCollection<ICompatibilityInfo> Details { get; }
        bool Compatible(IScene scene);
    }
}