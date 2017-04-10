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
        }

    }
}