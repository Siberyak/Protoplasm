using System.Collections.Generic;

namespace ConsoleApplication1.TestData
{
    public class CompetencesAbility : Ability
    {
        public IReadOnlyCollection<Competence> Competences { get; }

        public CompetencesAbility(IReadOnlyCollection<Competence> competences) : base(MappingType.Competences)
        {
            Competences = competences;
        }

        protected override ConformResult Conformable(BaseRequirement requirement)
        {
            var competencesRequirement = requirement as CompetencesRequirement;
            if (competencesRequirement == null)
                return ConformResult.Empty;

            return base.Conformable(requirement);
        }
    }


}