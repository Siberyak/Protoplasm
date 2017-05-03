using System;
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

        public CompetencesRequirement(IReadOnlyCollection<Competence> competences) 
        {
            Competences = Competences.New(competences);
        }

        public override CompatibilityType Compatible(IAbility ability)
        {
            var compatibility = Compatible(ability, true);
            return !compatibility.HasValue
                ? CompatibilityType.DependsOnScene
                : compatibility.Value
                    ? CompatibilityType.Always
                    : CompatibilityType.Never;

        }

        public override bool Compatible(IAbility ability, IScene scene)
        {
            var compatibility = Compatible(ability, false) ?? false;
            return compatibility;
        }

        private bool? Compatible(IAbility ability, bool checkMutable)
        {
            var competencesAbility = ability as CompetencesAbility;
            if (competencesAbility == null)
                return false;

            IEnumerable<CompetenceMatchingResult> result;
            if (!Competences.Acceptable(competencesAbility.Competences, out result))
                return false;

            return !checkMutable
                ? true
                : (!IsMutable && !competencesAbility.IsMutable ? true : default(bool?));
        }
    }
}