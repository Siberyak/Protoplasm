using System.Collections;

namespace Factorio.Lua.Reader
{
    class ItemNode : NodeData<Item>
    {
        public ItemNode(Item data) : base(data)
        {
        }

        protected override IList GetChildren()
        {
            return new ArrayList();
        }

        protected override object GetValue(string fieldName)
        {
            return Data.LocalizedName ?? Data.Name;
        }
    }
}