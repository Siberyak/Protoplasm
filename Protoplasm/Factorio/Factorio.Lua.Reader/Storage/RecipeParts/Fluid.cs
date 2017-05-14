using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("fluid", MemberSerialization = MemberSerialization.OptIn)]
    public partial class Fluid : TypedNamedIconedBase, IRecipePart
    { }
}