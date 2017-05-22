using MAS.Utils;

namespace Application1.Data
{
    public class ResourcesGroup : MembershipItemsContainer
    {
        public ResourcesGroup(string caption, params ResourcesGroup[] memberOf) : base(caption, memberOf)
        {
        }
    }
}