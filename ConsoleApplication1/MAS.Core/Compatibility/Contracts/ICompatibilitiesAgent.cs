using MAS.Core.Contracts;

namespace MAS.Core.Compatibility.Contracts
{
    public interface ICompatibilitiesAgent : IAgent
    {
        void Add(IAbilitiesCompatibilityInfo abilitiesCompatibilityInfo);
        void Add(IRequirementsCompatibilityInfo requirementsCompatibilityInfo);
    }
}