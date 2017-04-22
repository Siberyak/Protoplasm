using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1
{
    public static class CompatibilityHelper
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


        public static void Find(this BaseAgent requirements, IEnumerable<BaseAgent> abilities)
        {
            foreach (var agent in abilities)
            {
                var result = agent.Compatible(requirements);
                result.Details.All(x => x.Variants.Any());
            }
        }

        public static CompatibilityInfo[] Compatibility(this IEnumerable<BaseRequirement> requirements, IEnumerable<BaseAbility> abilities)
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