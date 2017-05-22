using System.Collections.Generic;

namespace MAS.Utils
{
    public static class MemberOfCompetencesExtender
    {
        public static Competences MemberOf(this Competences competences, out IEnumerable<Competence> result, params MembershipItem[] memberOf)
        {
            List<Competence> list = new List<Competence>();
            foreach (var membership in memberOf)
            {
                var competence = new MemberOfCompetence(membership);
                list.Add(competence);
                competences = competences.Add(competence);
            }

            result = list;

            return competences;
        }

        public static Competences MemberOf(this Competences competences, params MembershipItem[] memberOf)
        {
            IEnumerable<Competence> result;

            return MemberOf(competences, out result, memberOf);
        }

        
    }
}