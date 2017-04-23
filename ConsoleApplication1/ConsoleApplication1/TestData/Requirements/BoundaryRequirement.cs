using System.Linq;
using MAS.Core;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    { 
        public class BoundaryRequirement : Requirement
        {
            public Interval<TTime?> Start { get; }
            public Interval<TTime?> Finish { get; }
            public Interval<TDuration?> Duration { get; }

            public BoundaryRequirement(Interval<TTime?> start, Interval<TTime?> finish, Interval<TDuration?> duration) : base(MappingType.Boundary)
            {
                Start = start;
                Finish = finish;
                Duration = duration;
            }

            public override CompatibilityType Compatible(BaseAbility ability)
            {
                var calendarAbility = ability as CalendarAbility;
                return calendarAbility?.Compatible(this) ?? CompatibilityType.Never;
            }

            public override string ToString()
            {
                var start = Start == Interval<TTime?>.Empty ? null : $"S: {Start}";
                var finish = Finish == Interval<TTime?>.Empty ? null : $"F: {Finish}";
                var duration = Duration == Interval<TDuration?>.Empty ? null : $"D: {Duration}";
                var parts = new []{start, finish, duration};
                return string.Join(", ", parts.Where(x => x != null));
            }
        }

    }
}