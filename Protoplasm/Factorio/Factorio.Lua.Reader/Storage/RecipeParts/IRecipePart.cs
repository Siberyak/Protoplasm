using Factorio.Lua.Reader.Graph;
using Protoplasm.Utils.Graph;

namespace Factorio.Lua.Reader
{
    public interface IRecipePart : IIconed
    {
        string LocalizedName { get; }
        string Name { get; set; }

        string _Order { get; set; }
    }
}