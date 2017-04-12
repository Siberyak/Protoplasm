namespace ConsoleApplication1.TestData
{
    public class Department : MembershipItemsContainer
    {
        public Department(string caption, params Department[] memberOf) : base(caption, memberOf)
        {
        }

        public override bool MemberOfOrEqual(MembershipItem item)
        {
            return base.MemberOfOrEqual(item);
        }
    }
}