using System.Collections.Generic;

namespace ConsoleApplication1.TestData
{
    public abstract class MembershipItemsContainer : MembershipItem
    {
        protected readonly List<MembershipItem> Members = new List<MembershipItem>();

        protected MembershipItemsContainer(string caption, IEnumerable<MembershipItemsContainer> memberOf) : base(caption, memberOf)
        {
        }

        public void AddMember(MembershipItem member)
        {
            Members.Add(member);
        }
    }
}