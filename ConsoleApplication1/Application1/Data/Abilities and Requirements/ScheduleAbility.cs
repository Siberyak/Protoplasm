using System;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using MAS.Utils;
using Protoplasm.Calendars;

namespace Application1.Data
{
    public class ScheduleAbility : Ability
    {
        public WorkSchedule Schedule { get; }

        public override bool Compatible(IRequirement requirement, IScene scene)
        {
            return true;
        }

        public ScheduleAbility(WorkCalendar calendar)
        {
            Schedule = new WorkSchedule(calendar.CreateSchedule(TimeSpan.FromMinutes(15)));
        }

        public override CompatibilityType Compatible(IRequirement requirement)
        {
            return requirement is BoundaryRequirement ? CompatibilityType.DependsOnScene : CompatibilityType.Never;
        }
    }
}