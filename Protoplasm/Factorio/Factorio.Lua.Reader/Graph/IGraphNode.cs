using System.Collections.Generic;

namespace lua.reader.Graph
{

    public interface IDataItem<T>
    {
        T Data { get; }
    }

    public interface IGraphNode<T> : IGraphNode, IDataItem<T>
    {
    }

    public interface IGraphNode
    {

        IEnumerable<IGraphEdge> BackEdges { get; }
        IEnumerable<IGraphEdge> Edges { get; }
        IGraph Graph { get; }

    }
}