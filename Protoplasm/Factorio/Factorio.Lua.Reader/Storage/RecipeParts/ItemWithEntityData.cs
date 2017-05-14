using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("item-with-entity-data", MemberSerialization = MemberSerialization.OptIn)]
    public partial class ItemWithEntityData : TypedNamedIconedBase, IRecipePart
    {
        [JsonProperty("order")]
        public string _Order { get; set; }

    }
}