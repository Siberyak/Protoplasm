using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("item-group", MemberSerialization = MemberSerialization.OptIn)]
    public partial class ItemGroup : TypedNamedIconedBase, ILocalized
    {
        [JsonProperty("order")]
        public string _Order { get; set; }

        public string Category => "item-group-name";
        public object[] _LocalisedName => null;
    }
}