using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("constant-combinator", MemberSerialization = MemberSerialization.OptIn)]
    public partial class ConstantCombinator : Entity { }
}