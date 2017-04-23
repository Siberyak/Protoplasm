using System.Collections.Generic;
using System.Linq;

namespace MAS.Core
{
    public class CompatibilityInfo
    {
        public static CompatibilityInfo[] Empty = new CompatibilityInfo[0];
        public BaseRequirement Requirement;
        public Dictionary<CompatibilityType, BaseAbility[]> Variants;
        public override string ToString()
        {
            var variants = Variants.Count == 0 ? (object)CompatibilityType.Never : string.Join(", ", Variants.Select(x => $"{x.Key}:{x.Value.Length}"));
            return $"{Requirement} <- {variants}";
        }
    }
}