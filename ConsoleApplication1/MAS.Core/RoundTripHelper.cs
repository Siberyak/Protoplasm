using System.Collections;
using System.Collections.Generic;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Core
{
    public static class RoundTripHelper
    {

        public static bool TrySatisfy(this IRequirementsHolder requirements, IScene current, out IScene result)
        {
            result = current;
            var negotiator = requirements[current];

            IEnumerable<IHoldersCompatibilityInfo> infos = negotiator.Agent.CompatibilityInfos();

            foreach (var info in infos)
            {
                var branch = current.Branch();
                IScene variant;

                if (!info.Variate(branch, out variant))
                    continue;

                if (variant.Satisfaction.CompareTo(current.Satisfaction) > 0)
                    current = variant;
            }

            return result != null && current != result;
        }

        public static bool Variate(this IHoldersCompatibilityInfo info, IScene current, out IScene result)
        {
            var requirements = info.RequiremenetsHolder[current];
            var abilities = info.AbilitiesHolder[current];

            result = requirements.Variate(abilities);

            return result != null && current != result;
        }
    }
}