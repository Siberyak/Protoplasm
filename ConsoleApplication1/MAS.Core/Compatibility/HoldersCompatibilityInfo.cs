using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Core.Compatibility
{
    internal class HoldersCompatibilityInfo : IHoldersCompatibilityInfo, IRequirementsCompatibilityInfo, IAbilitiesCompatibilityInfo
    {
        private IEnumerable<ICompatibilityInfo> _scened;

        public IRequirementsHolder RequiremenetsHolder { get; }
        public IAbilitiesHolder AbilitiesHolder { get; }
        public CompatibilityType Compatibility { get; }
        public IReadOnlyCollection<ICompatibilityInfo> Details { get; }
        IRequirementsHolder IRequirementsCompatibilityInfo.Holder => RequiremenetsHolder;
        IAbilitiesHolder IAbilitiesCompatibilityInfo.Holder => AbilitiesHolder;

        public HoldersCompatibilityInfo(IRequirementsHolder requiremenetsHolder, IAbilitiesHolder abilitiesHolder, IReadOnlyCollection<ICompatibilityInfo> details)
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

            var array = Details.Where(x => x.Compatibility.Count > 0)
                .Where(x => !x.Compatibility.ContainsKey(CompatibilityType.Always) && x.Compatibility.ContainsKey(CompatibilityType.DependsOnScene))
                .ToArray();

            var r = array.Select(x => x.Requirement)
                .Distinct()
                .ToDictionary(x => x, x => RequiremenetsHolder.ToScene(x));

            var a = array.SelectMany(x => x.Compatibility[CompatibilityType.DependsOnScene])
                .Distinct()
                .ToDictionary(x => x, x => AbilitiesHolder.ToScene(x));

            _scened = array.Select
                (
                    x => new CompatibilityInfo
                        (
                        r[x.Requirement],
                        new Dictionary<CompatibilityType, IAbility[]>
                        {
                            {
                                CompatibilityType.DependsOnScene,
                                x.Compatibility[CompatibilityType.DependsOnScene].Select(y => a[y]).ToArray()
                            }
                        }
                        )
                )
                .ToArray();
        }

        public bool Compatible(IScene scene)
        {
            switch (Compatibility)
            {
                case CompatibilityType.Always:
                    return true;
                case CompatibilityType.DependsOnScene:
                    return _scened.All(x => x.Compatible(scene)); 
                case CompatibilityType.Never:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string ToString()
        {
            return $"R: [{RequiremenetsHolder} <-> A: [{AbilitiesHolder}] => [{Compatibility}]";
        }
    }
}