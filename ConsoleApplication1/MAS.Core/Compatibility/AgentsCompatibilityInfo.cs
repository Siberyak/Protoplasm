using System;
using System.Collections.Generic;
using MAS.Core.Compatibility.Contracts;

namespace MAS.Core.Compatibility
{
    internal class AgentsCompatibilityInfo : IRequirementsCompatibilityInfo, IAbilitiesCompatibilityInfo
    {
        public readonly IRequirementsHolder RequiremenetsHolder;
        public readonly IAbilitiesHolder AbilitiesHolder;
        public readonly CompatibilityType Compatibility;

        public readonly IReadOnlyCollection<CompatibilityInfo> Details;

        public AgentsCompatibilityInfo(IRequirementsHolder requiremenetsHolder, IAbilitiesHolder abilitiesHolder, IReadOnlyCollection<CompatibilityInfo> details)
        {
            if(requiremenetsHolder == null)
                throw new ArgumentNullException(nameof(requiremenetsHolder));
            if (abilitiesHolder == null)
                throw new ArgumentNullException(nameof(abilitiesHolder));
            if (details == null)
                throw new ArgumentNullException(nameof(details));

            RequiremenetsHolder = requiremenetsHolder;
            AbilitiesHolder = abilitiesHolder;
            Compatibility = details.ToCompatibilityType();
            Details = details;

            //requiremenetsHolder.CompatibilitiesAgent.Add((IAbilitiesCompatibilityInfo)this);
            //abilitiesHolder.CompatibilitiesAgent.Add((IRequirementsCompatibilityInfo)this);
        }

        IRequirementsHolder IRequirementsCompatibilityInfo.Holder => RequiremenetsHolder;
        IAbilitiesHolder IAbilitiesCompatibilityInfo.Holder => AbilitiesHolder;

        public override string ToString()
        {
            return $"R: [{RequiremenetsHolder} <-> A: [{AbilitiesHolder}] => [{Compatibility}]";
        }
    }




}