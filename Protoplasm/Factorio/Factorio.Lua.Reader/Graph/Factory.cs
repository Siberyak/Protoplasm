using System;
using System.Windows.Forms;
using lua.reader.Graph;
using Protoplasm.Utils.Graph;


namespace lua.reader.Graph
{
    public class Factory : INodesFactory, IEdgesFactory
    {
        #region Fields

        private readonly DataByKey<Type> EdgesByDataFuncs = new DataByKey<Type>();

        private readonly DataByKey<Type> EdgesByTypeFuncs = new DataByKey<Type>();

        private readonly DataByKey<Type> NodesByDataFuncs = new DataByKey<Type>();

        private readonly DataByKey<Type> NodesByTypeFuncs = new DataByKey<Type>();

        #endregion

        #region Public Methods and Operators

        public void RegisterCreateEdge<TData, TEdge>(Func<IGraph, IGraphNode, IGraphNode, TData, TEdge> createInstance)
            where TEdge : IGraphEdge<TData>
        {
            EdgesByDataFuncs.Add(typeof(TData), createInstance);
        }

        public void RegisterCreateEdge<TEdge>(Func<IGraph, IGraphNode, IGraphNode, TEdge> createInstance)
            where TEdge : IGraphEdge
        {
            EdgesByTypeFuncs.Add(typeof(TEdge), createInstance);
        }

        public void RegisterCreateNode<TData, TNode>(Func<IGraph, TData, TNode> createInstance)
            where TNode : IGraphNode<TData>
        {
            NodesByDataFuncs.Add(typeof(TData), createInstance);
        }

        public void RegisterCreateNode<TNode>(Func<IGraph, TNode> createInstance)
            where TNode : IGraphNode
        {
            NodesByTypeFuncs.Add(typeof(TNode), createInstance);
        }

        #endregion

        TNode INodesFactory.Create<TNode>(IGraph graph)
        {
            return NodesByTypeFuncs.Get<Func<IGraph, TNode>>(typeof(TNode))(graph);
        }

        TNode INodesFactory.Create<TNode, TData>(IGraph graph, TData data)
        {
            return (TNode)NodesByDataFuncs.Get<Func<IGraph, TData, IGraphNode<TData>>>(data.GetType())(graph, data);
        }

        TEdge IEdgesFactory.Create<TEdge>(IGraph graph, IGraphNode from, IGraphNode to)
        {
            return (TEdge)EdgesByTypeFuncs.Get<Func<IGraph, IGraphNode, IGraphNode, IGraphEdge>>(typeof(TEdge))(graph, from, to);
        }

        TEdge IEdgesFactory.Create<TEdge, TData>(IGraph graph, IGraphNode from, IGraphNode to, TData data)
        {
            return (TEdge) EdgesByDataFuncs.Get<Func<IGraph, IGraphNode, IGraphNode, TData, IGraphEdge<TData>>>(data.GetType())(graph, @from, to, data);
        }
    }

    public interface INodesFactory
    {
        TNode Create<TNode>(IGraph graph) where TNode : IGraphNode;
        TNode Create<TNode, TData>(IGraph graph, TData data) where TNode : IGraphNode<TData>;
    }
    public interface IEdgesFactory
    {
        TEdge Create<TEdge>(IGraph graph, IGraphNode from, IGraphNode to) where TEdge : IGraphEdge;
        TEdge Create<TEdge, TData>(IGraph graph, IGraphNode from, IGraphNode to, TData data) where TEdge : IGraphEdge<TData>;
    }
}