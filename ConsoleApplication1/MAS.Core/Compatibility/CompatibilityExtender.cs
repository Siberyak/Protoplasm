using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Core.Compatibility
{
    public static class CompatibilityExtender
    {
        public static CompatibilityType ToCompatibilityType(this IEnumerable<CompatibilityInfo> compatibilityInfo)
        {
            var infos = compatibilityInfo?.ToArray() ?? CompatibilityInfo.Empty;

            if (infos.Length == 0 || infos.Any(x => x.Variants.Count == 0 || x.Variants.ContainsKey(CompatibilityType.Never)))
                return CompatibilityType.Never;

            if (infos.Any(x => x.Variants.ContainsKey(CompatibilityType.DependsOnScene)))
                return CompatibilityType.DependsOnScene;

            if (infos.All(x => x.Variants.ContainsKey(CompatibilityType.Always)))
                return CompatibilityType.Always;

            throw new NotImplementedException();
        }

        public static CompatibilityInfo[] Compatibility(this IEnumerable<IRequirement> requirements, IEnumerable<IAbility> abilities)
        {
            var array = abilities.ToArray();
            var result = requirements
                .Select(
                    r => new CompatibilityInfo
                    {
                        Requirement = r,
                        Variants = array
                            .Select
                            (
                                a => new
                                {
                                    Ability = a,
                                    Result = r.Compatible(a)
                                }
                            )
                            .Where(x => x.Result != CompatibilityType.Never)
                            .GroupBy(x => x.Result)
                            .ToDictionary(x => x.Key, x => x.Select(y => y.Ability).ToArray())
                    }
                )
                .ToArray();

            return result;
        }
    }
}