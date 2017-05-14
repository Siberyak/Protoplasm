using System.Collections;

namespace Factorio.Lua.Reader
{
    class RecipeNode : NodeData<Recipe>
    {
        public RecipeNode(Recipe recipe) : base(recipe)
        {

        }

        protected override IList GetChildren()
        {
            return new ArrayList();
        }

        protected override object GetValue(string fieldName)
        {
            return Data.LocalizedName;
        }
    }
}