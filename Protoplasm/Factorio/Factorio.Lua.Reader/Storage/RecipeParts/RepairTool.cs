using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("repair-tool", MemberSerialization = MemberSerialization.OptIn)]
    public partial class RepairTool : TypedNamedIconedBase, IRecipePart
    {
        [JsonProperty("order")]
        public string _Order { get; set; }

    }
}