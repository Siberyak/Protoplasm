using Protoplasm.Utils.Graph;

namespace Factorio.Lua.Reader
{
    public interface ICrafter : INode, IStoragedData, ILocalized, IIconed
    {
        object[] _CraftingCategories { get; }

        double _CraftingSpeed { get; }
    }
}