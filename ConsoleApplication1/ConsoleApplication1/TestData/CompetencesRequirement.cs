using System.Collections.Generic;

namespace ConsoleApplication1.TestData
{
    public class CompetencesRequirement : Requirement
    {
        public IReadOnlyCollection<Competence> Competences { get; }

        public CompetencesRequirement(IReadOnlyCollection<Competence> competences) : base(MappingType.Competences)
        {
            Competences = competences;
        }
    }
}