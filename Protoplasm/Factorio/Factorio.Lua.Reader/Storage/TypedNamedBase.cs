using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TypedNamedBase : TypedBase
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        public virtual string LocalizedName => (this as ILocalized)?.LocalisedName() ?? Name;

        public override string ToString()
        {
            return $"{Type}: '{LocalizedName}'" ?? base.ToString();
        }
    }
}