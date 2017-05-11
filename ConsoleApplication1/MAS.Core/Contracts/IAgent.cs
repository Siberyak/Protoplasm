using System;
using System.Collections.Generic;
using MAS.Core.Compatibility.Contracts;

namespace MAS.Core.Contracts
{
    public interface IAgent : IRespondent, IRequirementsHolder, IAbilitiesHolder
    {
        void Initialize();
        IEnumerable<IHoldersCompatibilityInfo> CompatibilityInfos();

        INegotiator Negotiator(IScene scene);
    }
}