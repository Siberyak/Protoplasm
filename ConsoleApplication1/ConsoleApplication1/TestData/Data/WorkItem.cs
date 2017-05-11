using System.Collections.Generic;
using Protoplasm.PointedIntervals;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class WorkItem : Entity
        {
            private readonly BoundaryRequirement _boundaryRequirement;
            private readonly CompetencesRequirement _competencesRequirement;

            public Interval<TTime> Start => _boundaryRequirement.Start;
            public Interval<TTime> Finish => _boundaryRequirement.Finish;
            public Interval<TDuration> Duration => _boundaryRequirement.CalendarDuration;

            public IReadOnlyCollection<Competence> Competences => _competencesRequirement.Competences;

            // Parent-Group
            // Predecessors-Followers
            // Laboriousness

            public WorkItem
                (
                string caption,
                Interval<TTime> start,
                Interval<TTime> finish,
                Interval<TDuration> calendarDuration,
                Interval<TDuration> workingDuration,
                IReadOnlyCollection<Competence> competences
                )
                : base(caption)
            {
                _boundaryRequirement = new BoundaryRequirement(start, finish, calendarDuration, workingDuration);
                _competencesRequirement = new CompetencesRequirement(competences);
            }

            protected override IReadOnlyCollection<Requirement> GenerateRequirements()
            {
                return new Requirement[] { _boundaryRequirement, _competencesRequirement };
            }
        }


    }
}