using System.Collections;
using System.Linq;

namespace Factorio.Lua.Reader
{
    class ItemGroupNode : NodeData<ItemGroup>
    {
        public ItemGroupNode(ItemGroup data) : base(data)
        {
        }

        protected override IList GetChildren()
        {
            return Data.BackReferences.OfType<ItemSubGroupGroupEdge>()
                .Select(x => x.ItemSubGroup)
                .OrderBy(x => x.Order)
                .Select(x => new ItemSubGroupNode(x)).ToArray();
        }

        protected override object GetValue(string fieldName)
        {
            return Data.Name;
        }
    }
}