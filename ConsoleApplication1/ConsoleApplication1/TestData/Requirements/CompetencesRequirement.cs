using System.Collections.Generic;
using System.Linq;

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

        public override ConformType Conformable(BaseAbility ability)
        {
            var competencesAbility = ability as CompetencesAbility;
            if(competencesAbility == null)
                return ConformType.Imposible;

            IEnumerable<CompetenceMatchingResult> result;
            if(!Competences.Acceptable(competencesAbility.Competences, out result))
                return ConformType.Imposible;

            return IsMutable || competencesAbility.IsMutable ? ConformType.Posible : ConformType.Conform;
        }
    }
}