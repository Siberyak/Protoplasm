using Factorio.Lua.Reader.Graph;
using Protoplasm.Utils.Graph;

namespace Factorio.Lua.Reader
{
    public interface ICrafter : INode, IStoragedData
    {
        object[] _CraftingCategories { get; }

        string Name { get; }

      
    }
}