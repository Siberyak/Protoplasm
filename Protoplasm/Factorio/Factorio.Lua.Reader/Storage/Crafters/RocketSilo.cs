using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("rocket-silo", MemberSerialization = MemberSerialization.OptIn)]
    public partial class RocketSilo : TypedNamedBase, ICrafter
    {
        [JsonProperty("crafting_categories")]
        public object[] _CraftingCategories { get; set; }
    }
}