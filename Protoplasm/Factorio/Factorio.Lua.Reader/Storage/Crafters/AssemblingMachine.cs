using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("assembling-machine", MemberSerialization = MemberSerialization.OptIn)]
    public partial class AssemblingMachine : EntityWithHealth, ICrafter
    {
        [JsonProperty("crafting_categories")]
        public object[] _CraftingCategories { get; set; }

        [JsonProperty("crafting_speed")]
        public double _CraftingSpeed { get; set; }
    }
}