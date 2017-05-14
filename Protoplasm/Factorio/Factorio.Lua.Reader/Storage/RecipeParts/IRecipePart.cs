using System.ComponentModel;
using Factorio.Lua.Reader.Graph;
using Protoplasm.Utils.Graph;

namespace Factorio.Lua.Reader
{
    public interface IRecipePart : INode
    {
        string LocalizedName { get; }
        string Name { get; set; }

        [Browsable(false)]
        string _Icon { get; set; }

        string _Order { get; set; }
    }
}