using System.Collections.Generic;
using MAS.Core.Compatibility.Contracts;

namespace MAS.Core.Contracts
{
    public interface INegotiator : IRespondent, IAbilitiesHolder, IRequirementsHolder
    {
        IScene Scene { get; }

        ISatisfaction Satisfaction { get; }
        IAgent Agent { get; }
        bool IsSatisfied { get; }
        IScene Variate(INegotiator respondent);
    }
}