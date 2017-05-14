using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("rail-planner", MemberSerialization = MemberSerialization.OptIn)]
    public partial class RailPlanner : TypedNamedIconedBase, IRecipePart { }
}