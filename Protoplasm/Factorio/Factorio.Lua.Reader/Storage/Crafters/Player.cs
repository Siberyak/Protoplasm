using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("player", MemberSerialization = MemberSerialization.OptIn)]
    public partial class Player : EntityWithHealth, ICrafter
    {
        [JsonProperty("crafting_categories")]
        public object[] _CraftingCategories { get; set; }

    }
}