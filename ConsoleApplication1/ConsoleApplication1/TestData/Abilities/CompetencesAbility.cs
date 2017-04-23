using System.Collections.Generic;
using MAS.Core;

namespace ConsoleApplication1.TestData
{
    public class CompetencesAbility : Ability
    {
        public override bool IsMutable => false;

        public Competences Competences { get; }

        public CompetencesAbility(IReadOnlyCollection<Competence> competences) : base(MappingType.Competences)
        {
            Competences = Competences.New(competences);
        }

        public override CompatibilityType Compatible(BaseRequirement requirement)
        {
            var competencesRequirement = requirement as CompetencesRequirement;
            return competencesRequirement?.Compatible(this) ?? CompatibilityType.Never;
        }
    }


}