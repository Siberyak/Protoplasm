using System.Linq;
using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("item-subgroup", MemberSerialization = MemberSerialization.OptIn)]
    public partial class ItemSubGroup : TypedNamedBase
    {
        [JsonProperty("order")]
        public string Order { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }

        public override void ProcessLinks()
        {
            base.ProcessLinks();

            var @group = Storage.FindNode<ItemGroup>(x => x.Name == Group);
            Storage.Link<ItemSubGroupGroupEdge>(this, @group);

        }

        public ItemGroup ItemGroup => References.OfType<ItemSubGroupGroupEdge>().FirstOrDefault()?.ItemGroup;
    }
}