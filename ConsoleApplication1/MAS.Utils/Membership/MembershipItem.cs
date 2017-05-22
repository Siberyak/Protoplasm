using System.Collections.Generic;
using System.Linq;

namespace MAS.Utils
{
    public abstract class MembershipItem : Entity
    {
        protected readonly Competences MembershipCompetences;
        protected MembershipItem(string caption, IEnumerable<MembershipItemsContainer> memberOf) : base(caption)
        {
            var membership = memberOf.ToArray();
            MemberOf = membership;

            var competences = Competences.New();
            foreach (var item in membership)
            {
                competences = competences.MemberOf(item);
                item.AddMember(this);
            }

            MembershipCompetences = competences;
        }



        protected IEnumerable<MembershipItem> MemberOf { get; }

        public virtual bool MemberOfOrEqual(MembershipItem item)
        {
            return Equals(this, item) || MemberOf?.Any(x => x.MemberOfOrEqual(item)) == true;
        }

        public override string ToString()
        {
            return Caption ?? $"{base.ToString()}[{GetHashCode()}]";
        }
    }
}