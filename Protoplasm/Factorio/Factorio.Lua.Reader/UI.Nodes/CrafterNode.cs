using System.Collections;
using System.Linq;

namespace Factorio.Lua.Reader
{
    class CrafterNode : NodeData<ICrafter>
    {
        public CrafterNode(ICrafter crafter) : base(crafter)
        {

        }

        protected override IList GetChildren()
        {
            return Data.BackReferences.OfType<RecipeCategoryCrafterEdge>()
                .Select(x => x.RecipeCategory)
                .OrderBy(x => x.Name)
                .Select(x => new RecipeCategoryNode(x))
                .ToArray();
        }

        protected override object GetValue(string fieldName)
        {
            return Data.Name;
        }
    }
}