using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("tool", MemberSerialization = MemberSerialization.OptIn)]
    public partial class Tool : TypedNamedIconedBase, IRecipePart, ILocalized
    {
        string ILocalized.Category => "item-name";

        object[] ILocalized._LocalisedName => null;

        [JsonProperty("order")]
        public string _Order { get; set; }

    }
}