using System.Collections.Generic;
using MAS.Core;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
{
    public class CompetencesAbility : Ability
    {
        public override bool IsMutable => false;

        public Competences Competences { get; }

        public CompetencesAbility(IReadOnlyCollection<Competence> competences) 
        {
            Competences = Competences.New(competences);
        }

        public override CompatibilityType Compatible(IRequirement requirement)
        {
            return (requirement as CompetencesRequirement)?.Compatible(this) ?? CompatibilityType.Never;
        }

        public override bool Compatible(IRequirement requirement, IScene scene)
        {
            return (requirement as CompetencesRequirement)?.Compatible(this, scene) ?? false;
        }
    }


}