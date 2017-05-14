using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("furnace", MemberSerialization = MemberSerialization.OptIn)]
    public partial class Furnace : EntityWithHealth, ICrafter
    {
        [JsonProperty("crafting_categories")]
        public object[] _CraftingCategories { get; set; }
    }
}