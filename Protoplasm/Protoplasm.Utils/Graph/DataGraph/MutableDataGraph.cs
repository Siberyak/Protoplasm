using System;
using System.Collections.Generic;
using System.Linq;
using Protoplasm.Utils.Collections;

namespace Protoplasm.Utils.Graph
{
    public class MutableDataGraph : IMutableDataGraph
    {
        protected readonly IList<INode> _nodes = new List<INode>();

        protected internal readonly DataList<IEdge> EdgesDataList = new DataList<IEdge>();

        public virtual IDataNode<TNodeData> GetNode<TNodeData>(TNodeData data)
        {
            var node = _nodes.OfType<IDataNode<TNodeData>>().FirstOrDefault(x => x.Data.Equals(data));
            return node;
        }

        public virtual IDataEdge<TEdgeData> GetEdge<TEdgeData>(TEdgeData data)
        {
            var node = EdgesDataList.OfType<IDataEdge<TEdgeData>>().FirstOrDefault(x => x.Data.Equals(data));
            return node;
        }

        public virtual IDataNode<TNodeData> Add<TNodeData>(TNodeData data)
        {
            var node = GetNodesFactory<TNodeData>().Create(data);
            AddNode(node);
            return node;
        }

        public virtual IDataEdge<TEdgeData> Add<TEdgeData>(INode from, INode to, TEdgeData data, bool isBackreference = false)
        {
            if (@from == null)
                throw new ArgumentNullException("from");
            if (to == null)
                throw new ArgumentNullException("to");

            if (!_nodes.Contains(@from))
                throw new ArgumentException(@"данная нода не принадлежит этому графу", "from");

            if (!_nodes.Contains(to))
                throw new ArgumentException(@"данная нода не принадлежит этому графу", "to");

            var edge = GetEdgesFactory<TEdgeData>().Create(@from, to, data, isBackreference);
            AddEdge(edge);
            return edge;
        }

        public virtual void RemoveNode<TNodeData>(TNodeData data)
        {
            var node = GetNode(data);
            Remove(node);
        }

        public virtual void RemoveEdge<TEdgeData>(TEdgeData data)
        {
            var edge = GetEdge(data);
            Remove(edge);
        }

        public virtual IDataNodesFactory<TNodeData> GetNodesFactory<TNodeData>()
        {
            return Register<TNodeData>.GetNodesFactory(this);
        }

        public virtual IDataEdgesFactory<TEdgeData> GetEdgesFactory<TEdgeData>()
        {
            return Register<TEdgeData>.GetEdgesFactory(this);
        }

        public class Register<TData>
        {
            public static Func<MutableDataGraph, IDataNodesFactory<TData>> NodesFactoryFunc { get; set; } = graph => new DataNodesFactory<TData>(graph);

            public static Func<MutableDataGraph, IDataEdgesFactory<TData>> EdgesFactoryFunc { get; set; } = graph => new DataEdgesFactory<TData>(graph);

            public static IDataNodesFactory<TData> GetNodesFactory(MutableDataGraph dataGraph)
            {
                return NodesFactoryFunc.Invoke(dataGraph);
            }

            public static IDataEdgesFactory<TData> GetEdgesFactory(MutableDataGraph dataGraph)
            {
                return EdgesFactoryFunc.Invoke(dataGraph);
            }
        }

        #region implementation of IMutableDataGraph

        public IEnumerable<INode> Nodes
        {
            get { return _nodes; }
        }

        public IEnumerable<IEdge> Edges
        {
            get { return EdgesDataList; }
        }

        public void AddNode<TNodeData>(IDataNode<TNodeData> node)
        {
            if (!_nodes.Contains(node))
            {
                _nodes.Add(node);
                node.AfterAdd();
            }
        }

        public void AddEdge<TEdgeData>(IDataEdge<TEdgeData> edge)
        {
            if (!EdgesDataList.Contains(edge))
                EdgesDataList.Add(edge);
        }

        IDataNode<TNodeData> IMutableDataGraph.Add<TNodeData>(TNodeData data)
        {
            return Add(data);
        }

        IDataEdge<TEdgeData> IMutableDataGraph.Add<TEdgeData>(
            INode @from,
            INode to,
            TEdgeData data,
            bool isBackreference)
        {
            return Add(@from, to, data, isBackreference);
        }

        public virtual void Remove(INode node)
        {
            node.BeforeRemove();

            foreach (var reference in node.References.ToArray())
                Remove(reference);
            foreach (var reference in node.BackReferences.ToArray())
                Remove(reference);
            _nodes.Remove(node);
        }

        public virtual void Remove(IEdge edge)
        {
            EdgesDataList.Remove(edge);
        }

        #endregion

        public IEnumerable<T> Datas<T>(Func<T, bool> predicate = null)
        {
            var query = Nodes.OfType<IDataNode<T>>().Select(x => x.Data);
            return predicate == null ? query : query.Where(predicate);
        }
        public IEnumerable<IDataNode<T>> DataNodes<T>(Func<IDataNode<T>, bool> predicate = null)
        {
            var query = Nodes.OfType<IDataNode<T>>();
            return predicate == null ? query : query.Where(predicate);
        }
        public IEnumerable<IDataEdge<T>> DataEdges<T>(Func<IDataEdge<T>, bool> predicate = null)
        {
            var query = Edges.OfType<IDataEdge<T>>();
            return predicate == null ? query : query.Where(predicate);
        }
    }


    public static class GraphExtender
    {
        public static IEnumerable<INode> ReferencedNodes<TEdgeType>(this INode node, Func<TEdgeType, bool> predicate = null)
            where TEdgeType : IEdge
        {
            var query = node.References.OfType<TEdgeType>();
            var edges = predicate == null ? query : query.Where(predicate);
            return edges.Select(x => x.To);
        }
        public static IEnumerable<INode> ReferencedByNodes<TEdgeType>(this INode node, Func<TEdgeType, bool> predicate = null)
            where TEdgeType : IEdge
        {
            var query = node.BackReferences.OfType<TEdgeType>();
            var edges = predicate == null ? query : query.Where(predicate);
            return edges.Select(x => x.From);
        }


        public static IEnumerable<TNode> ReferencedNodes<TEdgeType, TNode>(this INode node, Func<TEdgeType, bool> predicate = null)
            where TNode : INode
            where TEdgeType : IEdge
        {
            return node.ReferencedNodes(predicate).OfType<TNode>();
        }
        public static IEnumerable<TNode> ReferencedByNodes<TEdge, TNode>(this INode node, Func<TEdge, bool> predicate = null)
            where TNode : INode
            where TEdge : IEdge
        {
            return node.ReferencedByNodes(predicate).OfType<TNode>();
        }
    }
}