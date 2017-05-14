using System.Collections.Generic;
using System.Linq;

namespace lua.reader.Graph
{
    public class GraphNode<T> : GraphNode, IGraphNode<T>
    {
        #region Constructors and Destructors

        public GraphNode(IGraph graph, T data)
            : base(graph)
        {
            Data = data;
        }

        #endregion

        #region Properties

        public T Data { get; }

        #endregion
    }

    public abstract class GraphNode : IGraphNode
    {
        #region Fields

        protected IGraph _graph;

        #endregion

        #region Constructors and Destructors

        protected GraphNode(IGraph graph)
        {
            _graph = graph;
        }

        #endregion

        #region Properties

        public IEnumerable<IGraphEdge> BackEdges => _graph.Edges.Where(x => x.To == this);

        public IEnumerable<IGraphEdge> Edges => _graph.Edges.Where(x => x.From == this);

        IGraph IGraphNode.Graph
        {
            get
            {
                return _graph;
            }
        }

        #endregion
    }
}