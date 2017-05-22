using System;
using System.Collections.Generic;
using MAS.Utils;

namespace Application1.Data
{
    public class WorkItem : Entity
    {
        private readonly BoundaryRequirement _boundaryRequirement;
        private readonly CompetencesRequirement _competencesRequirement;

        public WorkItem(string caption, IReadOnlyCollection<Competence> competences) : base(caption)
        {
            _competencesRequirement = new CompetencesRequirement(competences);

            var boundary = Boundary.Empty;

            boundary = boundary
                .StartAfter(DateTime.Today)
                .NotLonger(TimeSpan.FromHours(16));

            _boundaryRequirement = new BoundaryRequirement(boundary);
        }

        public Boundary Boundary => _boundaryRequirement.Boundary;
        public WorkItemAmount RequiredAmount => new WorkItemAmount(this, 8, TimeSpan.FromHours(1), false, false);

        protected override IReadOnlyCollection<Requirement> GenerateRequirements()
        {
            return new Requirement[]
            {
                _boundaryRequirement,
                _competencesRequirement,

            };
        }

    }
}