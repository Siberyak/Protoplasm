using Factorio.Lua.Reader.Graph;
using Protoplasm.Utils.Graph;

namespace Factorio.Lua.Reader
{
    public interface ICrafter : INode, IStoragedData, ILocalized
    {
        object[] _CraftingCategories { get; }

        string _Icon { get; }

    }
}