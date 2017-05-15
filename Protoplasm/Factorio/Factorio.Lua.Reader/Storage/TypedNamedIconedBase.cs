using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TypedNamedIconedBase : TypedNamedBase, IIconed
    {
        [JsonProperty("icon")]
        public string _Icon { get; set; }

    }
}