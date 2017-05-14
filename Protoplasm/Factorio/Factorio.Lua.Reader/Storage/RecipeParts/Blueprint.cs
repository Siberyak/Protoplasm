using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("blueprint", MemberSerialization = MemberSerialization.OptIn)]
    public partial class Blueprint : TypedNamedIconedBase, IRecipePart
    {
    }
}