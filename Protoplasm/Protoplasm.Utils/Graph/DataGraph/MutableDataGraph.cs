using System;
using System.Collections.Generic;
using System.Linq;
using KG.SE2.Utils.Collections;

namespace KG.SE2.Utils.Graph
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
            Add(node);
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
            Add(edge);
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

        public void Add<TNodeData>(IDataNode<TNodeData> node)
        {
            if (!_nodes.Contains(node))
            {
                _nodes.Add(node);
                node.AfterAdd();
            }
        }

        public void Add<TEdgeData>(IDataEdge<TEdgeData> edge)
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
    }
}