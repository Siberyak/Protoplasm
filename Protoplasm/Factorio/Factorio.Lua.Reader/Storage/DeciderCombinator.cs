using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("decider-combinator", MemberSerialization = MemberSerialization.OptIn)]
    public partial class DeciderCombinator : Entity
    {
    }
}