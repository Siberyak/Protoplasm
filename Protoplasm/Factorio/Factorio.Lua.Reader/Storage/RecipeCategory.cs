using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Protoplasm.Utils.Graph;

namespace Factorio.Lua.Reader
{
    [JsonObject("recipe-category", MemberSerialization = MemberSerialization.OptIn)]
    public partial class RecipeCategory : TypedNamedBase
    {
        public override void ProcessLinks()
        {
            base.ProcessLinks();

            var crafters = Storage.Nodes.OfType<ICrafter>().ToArray();
            var craftersByName = crafters.Where(x => x._CraftingCategories.Contains(Name)).ToArray();
            if (craftersByName.Length != 0)
            {
                
            }

            foreach (var crafter in craftersByName)
            {
                Storage.Link<RecipeCategoryCrafterEdge>(this, crafter);
            }
        }

        public IEnumerable<ICrafter> Crafters => References.OfType<RecipeCategoryCrafterEdge>().Select(x => x.Crafter);
    }

    public class RecipeCategoryCrafterEdge : EdgeBase
    {
        public RecipeCategoryCrafterEdge(Storage storage, RecipeCategory recipeCategory, ICrafter crafter) : base(storage, recipeCategory, (Base)crafter)
        {
        }

        public RecipeCategory RecipeCategory => (RecipeCategory) _from;
        public ICrafter Crafter => (ICrafter) _to;
    }
}