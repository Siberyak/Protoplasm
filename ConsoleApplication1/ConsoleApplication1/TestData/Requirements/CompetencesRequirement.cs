using System.Collections.Generic;
using System.Linq;
using MAS.Core;

namespace ConsoleApplication1.TestData
{
    public class CompetencesRequirement : Requirement
    {
        public override bool IsMutable => false;
        public Competences Competences { get; }

        public CompetencesRequirement(IReadOnlyCollection<Competence> competences) : base(MappingType.Competences)
        {
            Competences = Competences.New(competences);
        }

        public override CompatibilityType Compatible(BaseAbility ability)
        {
            var competencesAbility = ability as CompetencesAbility;
            if(competencesAbility == null)
                return CompatibilityType.Never;

            IEnumerable<CompetenceMatchingResult> result;
            if(!Competences.Acceptable(competencesAbility.Competences, out result))
                return CompatibilityType.Never;

            return IsMutable || competencesAbility.IsMutable ? CompatibilityType.DependsOnScene : CompatibilityType.Always;
        }
    }
}