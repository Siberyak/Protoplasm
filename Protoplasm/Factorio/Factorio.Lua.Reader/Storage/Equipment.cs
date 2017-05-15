using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("Prototype/Equipment", MemberSerialization = MemberSerialization.OptIn)]
    public partial class Equipment : Item
    {
        public override string Category => "equipment-name";
    }
}