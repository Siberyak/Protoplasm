using System.Collections;
using System.Linq;

namespace Factorio.Lua.Reader
{
    class Root : VirtualTreeListData
    {

        public Root(Storage storage) : base(storage)
        {
        }

        protected override IList GetChildren()
        {

            return new[]
            {
                new StoragedNode(Storage, "Items", s => s.ItemGroups.OrderBy(x => x._Order).Select(x => new ItemGroupNode(x))),
                new StoragedNode(Storage, "Recipies", s => s.RecipeCategories.Select(x => new RecipeCategoryNode(x))),
                new StoragedNode(Storage, "Crafters", s => s.Nodes.OfType<ICrafter>().Select(x => new CrafterNode(x))),
                new N(Storage, "All Nodes"), 
            };
        }


        protected override object GetValue(string fieldName)
        {
            return null;
        }

        public override object NodeData => null;
    }
}