using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Core.Compatibility
{
    public static class CompatibilityExtender
    {
        public static CompatibilityType ToCompatibilityType(this IEnumerable<ICompatibilityInfo> compatibilityInfo)
        {
            var infos = compatibilityInfo?.ToArray() ?? CompatibilityInfo.Empty;

            if (infos.Length == 0 || infos.Any(x => x.Compatibility.Count == 0 || x.Compatibility.ContainsKey(CompatibilityType.Never)))
                return CompatibilityType.Never;

            if (infos.Any(x => x.Compatibility.ContainsKey(CompatibilityType.DependsOnScene)))
                return CompatibilityType.DependsOnScene;

            if (infos.All(x => x.Compatibility.ContainsKey(CompatibilityType.Always)))
                return CompatibilityType.Always;

            throw new NotImplementedException();
        }

        public static IHoldersCompatibilityInfo Compatibility(this IRequirementsHolder requirements, IAbilitiesHolder abilities)
        {
            var details = requirements
                .Requirements.Compatibility(abilities.Abilities)
                .ToArray();

            return new HoldersCompatibilityInfo(requirements, abilities, details);
        }

        public static ICompatibilityInfo[] Compatibility(this IEnumerable<IRequirement> requirements, IEnumerable<IAbility> abilities)
        {
            var array = abilities.ToArray();
            var result = requirements
                .Select(
                    requirement => new CompatibilityInfo
                    (
                        requirement,
                        array
                            .Select
                            (
                                a => new
                                {
                                    Ability = a,
                                    Result = requirement.Compatible(a)
                                }
                            )
                            .Where(x => x.Result != CompatibilityType.Never)
                            .GroupBy(x => x.Result)
                            .ToDictionary(x => x.Key, x => x.Select(y => y.Ability).ToArray())
                    )
                    
                )
                .ToArray();

            return result;
        }

        public static IRequirementsHolder Requirements<T>(this T candidate)
            where T : IRequirementsHolder
        {
            return candidate;
        }
        public static IAbilitiesHolder Abilities<T>(this T candidate)
            where T : IAbilitiesHolder
        {
            return candidate;
        }
    }


}