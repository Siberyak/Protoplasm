using System.Collections;
using System.Linq;

namespace Factorio.Lua.Reader
{
    class ItemSubGroupNode : NodeData<ItemSubGroup>
    {
        public ItemSubGroupNode(ItemSubGroup data) : base(data)
        {
        }

        protected override IList GetChildren()
        {
            return Data.BackReferences.OfType<ItemSubGroupEdge>()
                .Select(x => x.Item)
                .OrderBy(x => x._Order)
                .Select(x => new ItemNode(x)).ToArray();
        }

        protected override object GetValue(string fieldName)
        {
            return Data.Name;
        }
    }
}