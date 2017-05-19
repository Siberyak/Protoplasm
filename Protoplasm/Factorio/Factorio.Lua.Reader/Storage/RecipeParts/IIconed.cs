using System.ComponentModel;
using Protoplasm.Graph;

namespace Factorio.Lua.Reader
{
    public interface IIconed : INode, IStoragedData
    {
        [Browsable(false)]
        string _Icon { get; set; }
    }
}