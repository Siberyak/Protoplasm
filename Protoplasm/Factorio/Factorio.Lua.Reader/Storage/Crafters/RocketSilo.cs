using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("rocket-silo", MemberSerialization = MemberSerialization.OptIn)]
    public partial class RocketSilo : TypedNamedIconedBase, ICrafter
    {
        [JsonProperty("crafting_categories")]
        public object[] _CraftingCategories { get; set; }

        public string Category => "entity-name";
        public object[] _LocalisedName => null;



    }
}