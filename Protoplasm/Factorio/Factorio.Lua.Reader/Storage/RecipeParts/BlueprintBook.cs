using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("blueprint-book", MemberSerialization = MemberSerialization.OptIn)]
    public partial class BlueprintBook : TypedNamedIconedBase, IRecipePart
    { }
}