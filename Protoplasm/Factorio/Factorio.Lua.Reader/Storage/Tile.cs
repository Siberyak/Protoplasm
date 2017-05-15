using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    [JsonObject("tile", MemberSerialization = MemberSerialization.OptIn)]
    public partial class Tile : TypedNamedBase, ILocalized
    {
        public string Category => "tile-name";
        public object[] _LocalisedName => null;
    }
}