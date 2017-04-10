using System.Collections.Generic;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {


        public class WorkItem : Entity
        {
            private readonly BoundaryRequirement _boundaryRequirement;
            private readonly CompetencesRequirement _competencesRequirement;

            public Interval<TTime?> Start => _boundaryRequirement.Start;
            public Interval<TTime?> Finish => _boundaryRequirement.Finish;
            public Interval<TDuration?> Duration => _boundaryRequirement.Duration;

            public IReadOnlyCollection<Competence> Competences => _competencesRequirement.Competences;


            //Laboriousness


            public WorkItem(Interval<TTime?> start, Interval<TTime?> finish, Interval<TDuration?> duration, IReadOnlyCollection<Competence> competences)
            {
                _boundaryRequirement = new BoundaryRequirement(start, finish, duration);
                _competencesRequirement = new CompetencesRequirement(competences);
            }

            protected override IReadOnlyCollection<Requirement> GenerateRequirements()
            {
                return new Requirement[] {_boundaryRequirement, _competencesRequirement};
            }
        }


    }
}