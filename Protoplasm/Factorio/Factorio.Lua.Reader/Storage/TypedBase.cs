using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TypedBase : Base
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}