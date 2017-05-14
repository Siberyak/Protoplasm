namespace Factorio.Lua.Reader
{
    public class RecipePartEdge : EdgeBase
    {
        public RecipePartEdge(Storage storage, Recipe recipe, IRecipePart item) : base(storage, recipe, (Base)item)
        {
        }

        public Direction Direction { get; set; }
        public object Amount { get; set; }

        public Recipe Recipe => (Recipe) _from;
        public IRecipePart Part => (IRecipePart) _to;

        public override string ToString()
        {
            return $"[{GetType().Name}] {_from}: {Direction} {Amount} {_to}";
        }
    }


    public class RecipeItemSubGroupEdge : EdgeBase
    {
        public RecipeItemSubGroupEdge(Storage storage, Recipe @from, ItemSubGroup to) : base(storage, @from, to)
        {
        }

        public Recipe Recipe => (Recipe) _from;
        public ItemSubGroup ItemSubGroup => (ItemSubGroup) _to;
    }
    public class RecipeRecipeCategoryEdge : EdgeBase
    {
        public RecipeRecipeCategoryEdge(Storage storage, Recipe recipe, RecipeCategory recipeCategory) : base(storage, recipe, recipeCategory)
        {
        }

        public new Recipe From => (Recipe) _from;
        public new RecipeCategory To => (RecipeCategory)_to;
    }
}