using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TypedNamedBase : TypedBase
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        public virtual string LocalizedName => Name;

        public override string ToString()
        {
            return $"{Type}: '{Name}'" ?? base.ToString();
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class TypedNamedIconedBase : TypedNamedBase
    {
        [JsonProperty("icon")]
        public string _Icon { get; set; }

    }
}