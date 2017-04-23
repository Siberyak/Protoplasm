using System;
using System.Collections.Generic;

namespace MAS.Core
{
    public interface IRequiremetsCompatibilityInfo
    {
        BaseAgent Agent { get; }
    }

    public interface IAbilitiesCompatibilityInfo
    {
        BaseAgent Agent { get; }
    }

    public class AgentsCompatibilityInfo : IRequiremetsCompatibilityInfo, IAbilitiesCompatibilityInfo
    {
        public readonly BaseAgent RequiremenetsAgent;
        public readonly BaseAgent AbilitiesAgent;
        public readonly CompatibilityType Compatibility;

        public readonly IReadOnlyCollection<CompatibilityInfo> Details;

        public AgentsCompatibilityInfo(BaseAgent requiremenetsAgent, BaseAgent abilitiesAgent, IReadOnlyCollection<CompatibilityInfo> details)
        {
            if(requiremenetsAgent == null)
                throw new ArgumentNullException(nameof(requiremenetsAgent));
            if (abilitiesAgent == null)
                throw new ArgumentNullException(nameof(abilitiesAgent));
            if (details == null)
                throw new ArgumentNullException(nameof(details));

            RequiremenetsAgent = requiremenetsAgent;
            AbilitiesAgent = abilitiesAgent;
            Compatibility = details.ToCompatibilityType();
            Details = details;

            requiremenetsAgent.AddCompatibilityInfo((IAbilitiesCompatibilityInfo)this);
            abilitiesAgent.AddCompatibilityInfo((IRequiremetsCompatibilityInfo)this);
        }

        BaseAgent IRequiremetsCompatibilityInfo.Agent => RequiremenetsAgent;

        BaseAgent IAbilitiesCompatibilityInfo.Agent => AbilitiesAgent;

        public override string ToString()
        {
            return $"R: [{RequiremenetsAgent} <-> A: [{AbilitiesAgent}] => [{Compatibility}]";
        }
    }




}