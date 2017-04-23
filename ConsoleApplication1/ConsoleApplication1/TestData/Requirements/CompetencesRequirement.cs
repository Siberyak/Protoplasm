using System.Collections.Generic;
using System.Linq;
using MAS.Core;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

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

        public override CompatibilityType Compatible(IAbility ability)
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