using System;
using System.Linq;
using MAS.Core;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    { 
        public class BoundaryRequirement : Requirement
        {
            public Interval<TTime?> Start { get; }
            public Interval<TTime?> Finish { get; }
            public Interval<TDuration?> Duration { get; }

            public BoundaryRequirement(Interval<TTime?> start, Interval<TTime?> finish, Interval<TDuration?> duration) 
                
            {
                Start = start;
                Finish = finish;
                Duration = duration;
            }



            public override CompatibilityType Compatible(IAbility ability)
            {
                return IsCompatible(ability as CalendarAbility);
            }

            public override bool Compatible(IAbility ability, IScene scene)
            {
                return IsCompatible(ability as ScheduleAbility, scene);
            }

            private CompatibilityType IsCompatible(CalendarAbility ability)
            {
                return ability?.Compatible(this) ?? CompatibilityType.Never;
            }

            private bool IsCompatible(ScheduleAbility ability, IScene scene)
            {
                return ability?.Compatible(this, scene) ?? false;
            }


            public override string ToString()
            {
                var start = Start == Interval<TTime?>.Empty ? null : $"S: {Start}";
                var finish = Finish == Interval<TTime?>.Empty ? null : $"F: {Finish}";
                var duration = Duration == Interval<TDuration?>.Empty ? null : $"D: {Duration}";
                var parts = new []{start, finish, duration};
                return $"Boundary: [{string.Join(", ", parts.Where(x => x != null))}]";
            }

            public override IRequirement ToScene()
            {
                return this;
            }
        }

    }
}