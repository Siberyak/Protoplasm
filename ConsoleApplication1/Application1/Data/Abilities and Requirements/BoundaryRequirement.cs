using System;
using System.Linq;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using MAS.Utils;
using Protoplasm.PointedIntervals;

namespace Application1.Data
{
    public class BoundaryRequirement : Requirement
    {
        internal readonly Boundary Boundary;
        public Interval<DateTime> Start => Boundary.Start;
        public Interval<DateTime> Finish => Boundary.Finish;
        public Interval<TimeSpan> TotalDuration => Boundary.TotalDuration;
        public Interval<TimeSpan> Duration => Boundary.Duration;

        public BoundaryRequirement(Boundary boundary)
        {
            Boundary = boundary;
        }

        public override CompatibilityType Compatible(IAbility ability)
        {
            return IsCompatible(ability as ScheduleAbility);
        }

        public override bool Compatible(IAbility ability, IScene scene)
        {
            return IsCompatible(ability as ScheduleAbility, scene);
        }

        private CompatibilityType IsCompatible(ScheduleAbility ability)
        {
            return ability?.Compatible(this) ?? CompatibilityType.Never;
        }

        private bool IsCompatible(ScheduleAbility ability, IScene scene)
        {
            return ability?.Compatible(this, scene) ?? false;
        }


        public override string ToString()
        {
            return Boundary.ToString();
        }

        public override IRequirement ToScene()
        {
            return this;
        }
    }
}
