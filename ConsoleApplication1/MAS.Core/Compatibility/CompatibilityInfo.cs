using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Core.Compatibility
{
    internal class CompatibilityInfo : ICompatibilityInfo
    {
        public static ICompatibilityInfo[] Empty = new ICompatibilityInfo[0];

        public CompatibilityInfo(IRequirement requirement, IReadOnlyDictionary<CompatibilityType, IAbility[]> compatibility)
        {
            if (requirement == null)
                throw new ArgumentNullException(nameof(requirement));
            if (compatibility == null)
                throw new ArgumentNullException(nameof(compatibility));

            Requirement = requirement;
            Compatibility = compatibility;
        }

        public IRequirement Requirement { get; }
        public IReadOnlyDictionary<CompatibilityType, IAbility[]> Compatibility { get; }
        public bool Compatible(IScene scene)
        {
            return CompatibleAbilties(scene).Length > 0;
        }

        private IAbility[] CompatibleAbilties(IScene scene)
        {
            IAbility[] abilities = new IAbility[0];

            if (Compatibility.Count == 0)
                return abilities;

            if (Compatibility.TryGetValue(CompatibilityType.Always, out abilities)
                && abilities != null && abilities.Length > 0)
                return abilities;

            if (!Compatibility.TryGetValue(CompatibilityType.DependsOnScene, out abilities)
                || abilities == null || abilities.Length == 0)
            {
                return abilities;
            }

            abilities = abilities.Where(x => Requirement.Compatible(x, scene)).ToArray();
            return abilities;
        }

        public override string ToString()
        {
            var variants = Compatibility.Count == 0 ? (object)CompatibilityType.Never : string.Join(", ", Compatibility.Select(x => $"{x.Key}:{x.Value.Length}"));
            return $"{Requirement} <- {variants}";
        }
    }
}