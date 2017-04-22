using System.Collections.Generic;

namespace ConsoleApplication1
{
    public class AgentsCompatibilityInfo
    {
        public readonly BaseAgent RequiremenetsAgent;
        public readonly BaseAgent AbilitiesAgent;
        public readonly CompatibilityType Compatibility;

        public readonly IReadOnlyCollection<CompatibilityInfo> Details;

        public AgentsCompatibilityInfo(BaseAgent requiremenetsAgent, BaseAgent abilitiesAgent, IReadOnlyCollection<CompatibilityInfo> details)
        {
            RequiremenetsAgent = requiremenetsAgent;
            AbilitiesAgent = abilitiesAgent;
            Compatibility = details.ToCompatibilityType();
            Details = details;
        }

        public override string ToString()
        {
            return $"R: [{RequiremenetsAgent} <-> A: [{AbilitiesAgent}] => [{Compatibility}]";
        }
    }
}