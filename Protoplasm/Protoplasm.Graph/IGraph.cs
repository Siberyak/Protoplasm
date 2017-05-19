using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Protoplasm.Graph
{
    public interface IGraph
    {
        IEnumerable<INode> Nodes { get; }
        IEnumerable<IEdge> Edges { get; }
    }

    public interface IMutableGraph : IGraph
    {
	    INode Add();
	    IEdge Add(INode from, INode to, bool isBackreference = false);
		void Remove(INode node);
		void Remove(IEdge edge);
	}
}