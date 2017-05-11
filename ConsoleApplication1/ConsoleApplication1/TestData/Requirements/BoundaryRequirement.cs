using System;
using System.Linq;
using MAS.Core;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using Protoplasm.PointedIntervals;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    { 
        public class BoundaryRequirement : Requirement
        {
            public Interval<TTime> Start { get; }
            public Interval<TTime> Finish { get; }
            public Interval<TDuration> CalendarDuration { get; }
            public Interval<TDuration> WorkingDuration { get; }

            public BoundaryRequirement(Interval<TTime> start, Interval<TTime> finish, Interval<TDuration> calendarDuration, Interval<TDuration> workingDuration) 
                
            {
                Start = start;
                Finish = finish;
                CalendarDuration = calendarDuration;
                WorkingDuration = workingDuration;
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
                var start = Start == Interval<TTime>.Undefined ? null : $"S: {Start}";
                var finish = Finish == Interval<TTime>.Undefined ? null : $"F: {Finish}";
                var duration = CalendarDuration == Interval<TDuration>.Undefined ? null : $"D: {CalendarDuration}";
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