using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("fluid", MemberSerialization = MemberSerialization.OptIn)]
    public partial class Fluid : TypedNamedIconedBase, IRecipePart, ILocalized
    {
        public string Category => "fluid-name";
        public object[] _LocalisedName => null;


        [JsonProperty("order")]
        public string _Order { get; set; }

    }
}