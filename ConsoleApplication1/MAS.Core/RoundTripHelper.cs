using System;
using System.Collections;
using System.Collections.Generic;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Core
{
    public static class RoundTripHelper
    {
        private delegate IScene VariateDelegate(INegotiator requirements, INegotiator abilities);

        private delegate bool HolderVariateDelegate(IHoldersCompatibilityInfo info, IScene current, out IScene result);

        private static bool TrySatisfy(this IHolder holder, IScene current, out IScene result, HolderVariateDelegate variate)
        {
            result = current;
            var negotiator = holder[current];

            IEnumerable<IHoldersCompatibilityInfo> infos = negotiator.Agent.CompatibilityInfos();

            foreach (var info in infos)
            {
                var branch = current.Branch();
                IScene variant;

                if (!variate(info, branch, out variant))
                    continue;

                if (variant.Satisfaction.CompareTo(current.Satisfaction) > 0)
                    current = variant;
            }

            return result != null && current != result;
        }

        private static bool Variate(IHoldersCompatibilityInfo info, IScene current, out IScene result, VariateDelegate variate)
        {
            var requirements = info.RequiremenetsHolder[current];
            var abilities = info.AbilitiesHolder[current];
            
            result = variate(requirements, abilities);

            return result != null && current != result;
        }

        public static bool TrySatisfy(this IAbilitiesHolder abilities, IScene current, out IScene result)
        {
            return TrySatisfy(abilities, current, out result, VariateRequirements);
        }

        public static bool TrySatisfy(this IRequirementsHolder requirements, IScene current, out IScene result)
        {
            return TrySatisfy(requirements, current, out result, VariateAbilities);
        }

        public static bool VariateAbilities(this IHoldersCompatibilityInfo info, IScene current, out IScene result)
        {
            return Variate(info, current, out result, (requirements, abilities) => requirements.Variate(abilities));
        }

        public static bool VariateRequirements(this IHoldersCompatibilityInfo info, IScene current, out IScene result)
        {
            return Variate(info, current, out result, (requirements, abilities) => abilities.Variate(requirements));
        }
    }
}