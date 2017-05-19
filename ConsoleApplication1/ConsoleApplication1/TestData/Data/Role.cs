using MAS.Utils;

namespace ConsoleApplication1.TestData
{
    public class Role : MembershipItemsContainer
    {
        public Role(string caption = null) : base(caption, new Role[0])
        {
        }

        public override bool MemberOfOrEqual(MembershipItem item)
        {
            return Equals(this, item);
        }
    }
}