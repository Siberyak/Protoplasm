using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("tool", MemberSerialization = MemberSerialization.OptIn)]
    public partial class Tool : TypedNamedIconedBase, IRecipePart, ILocalized
    {
        string ILocalized.Category => "item-name";

        object[] ILocalized._LocalisedName => null;

        public override string LocalizedName => this.LocalisedName() ?? Name;
        [JsonProperty("order")]
        public string _Order { get; set; }

    }
}