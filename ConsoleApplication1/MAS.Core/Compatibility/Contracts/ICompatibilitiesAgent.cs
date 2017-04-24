using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using MAS.Core.Contracts;

namespace MAS.Core.Compatibility.Contracts
{
    public interface ICompatibilitiesAgent : IAgent
    {
        //IEnumerable<IAbilitiesHolder> AbilitiesHolders { get; }


        void Add(IAbilitiesCompatibilityInfo abilitiesCompatibilityInfo);
        void Add(IRequirementsCompatibilityInfo requirementsCompatibilityInfo);
    }
}