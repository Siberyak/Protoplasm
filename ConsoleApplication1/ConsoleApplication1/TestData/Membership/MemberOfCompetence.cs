using MAS.Utils;

namespace ConsoleApplication1.TestData
{
    public class MemberOfCompetence : Competence
    {
        public MemberOfCompetence(MembershipItem membershipItem)
        {
            MembershipItem = membershipItem;
        }

        public MembershipItem MembershipItem { get; }
        protected override object CompetenceKey => GetType();
        protected override Competence CompareValues(Competence other, bool backward = false)
        {
            if (backward)
                return null;

            var competence = other as MemberOfCompetence;
            return competence?.MembershipItem.MemberOfOrEqual(MembershipItem) == true ? competence : null;
        }

        public override string ToString()
        {
            return $"Member of [{MembershipItem}]";
        }
    }
}