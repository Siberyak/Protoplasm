using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility.Contracts;

namespace MAS.Core.Compatibility
{
    public class CompatibilityInfo
    {
        public static CompatibilityInfo[] Empty = new CompatibilityInfo[0];

        public IRequirement Requirement;
        public Dictionary<CompatibilityType, IAbility[]> Variants;
        public override string ToString()
        {
            var variants = Variants.Count == 0 ? (object)CompatibilityType.Never : string.Join(", ", Variants.Select(x => $"{x.Key}:{x.Value.Length}"));
            return $"{Requirement} <- {variants}";
        }
    }
}