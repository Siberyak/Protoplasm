namespace Factorio.Lua.Reader
{
    public class TechnologyUnlockRecipeEdge : EdgeBase
    {
        public TechnologyUnlockRecipeEdge(Storage storage, Technology technology, Recipe recipe) : base(storage, technology, recipe)
        {
        }

        public Technology Technology => (Technology)_from;
        public Recipe Recipe => (Recipe)_to;
    }
}