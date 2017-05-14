using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("item-group", MemberSerialization = MemberSerialization.OptIn)]
    public partial class ItemGroup : TypedNamedIconedBase
    {
    }
}