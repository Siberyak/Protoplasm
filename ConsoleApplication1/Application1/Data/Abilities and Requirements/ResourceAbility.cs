using System;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using MAS.Utils;

namespace Application1.Data
{
    public class ResourceAbility : Ability
    {
        internal readonly Resource Resource;

        public ResourceAbility(Resource resource)
        {
            Resource = resource;
        }

        public override bool Compatible(IRequirement requirement, IScene scene)
        {
            throw new NotImplementedException();
        }

        public override CompatibilityType Compatible(IRequirement requirement)
        {
            return (requirement as ResourceRequirement)?.Resource == Resource
                ? CompatibilityType.Always
                : CompatibilityType.Never;
        }
    }
}