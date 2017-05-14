using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("deconstruction-item", MemberSerialization = MemberSerialization.OptIn)]
    public partial class DeconstructionItem : TypedNamedIconedBase, IRecipePart
    {
        
    }
}