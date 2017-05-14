using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("Prototype/EntityWithHealth", MemberSerialization = MemberSerialization.OptIn)]
    public partial class EntityWithHealth : Entity
    {

    }
}