using System.Collections;
using System.Linq;

namespace Factorio.Lua.Reader
{
    class RecipeCategoryNode : NodeData<RecipeCategory>
    {
        public RecipeCategoryNode(RecipeCategory recipeCategory) : base(recipeCategory)
        {

        }

        protected override IList GetChildren()
        {
            return Data.BackReferences.OfType<RecipeRecipeCategoryEdge>()
                .Select(x => x.From)
                .OrderBy(x => x._Order)
                .Select(x => new RecipeNode(x))
                .ToArray();
        }

        protected override object GetValue(string fieldName)
        {
            return Data.Name;
        }
    }
}