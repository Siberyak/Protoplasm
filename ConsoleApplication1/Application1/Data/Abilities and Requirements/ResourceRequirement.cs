using System;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using MAS.Utils;
using Protoplasm.PointedIntervals;

namespace Application1.Data
{
    public class ResourceRequirement : Requirement
    {
        internal readonly Resource Resource;

        public ResourceRequirement(Resource resource)
        {
            Resource = resource;
        }

        public override CompatibilityType Compatible(IAbility ability)
        {
            return (ability as ResourceAbility)?.Resource == Resource
                ? CompatibilityType.Always
                : CompatibilityType.Never;
        }

        public override bool Compatible(IAbility ability, IScene scene)
        {
            throw new System.NotImplementedException();
        }
    }
}