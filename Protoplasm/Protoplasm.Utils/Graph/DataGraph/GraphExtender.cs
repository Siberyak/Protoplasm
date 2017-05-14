using System;
using System.Collections.Generic;
using System.Linq;

namespace Protoplasm.Utils.Graph
{
    public static class GraphExtender
    {

        public static IEnumerable<TEdge> Edges<TEdge>(this IGraph graph, Func<TEdge, bool> predicate = null)
            where TEdge : IEdge
        {
            var q = graph.Edges.OfType<TEdge>();
            if (predicate != null)
                q = q.Where(predicate);
            return q;
        }

        public static IEnumerable<TNode> Nodes<TNode>(this IGraph graph, Func<TNode, bool> predicate = null)
        {
            var q = graph.Nodes.OfType<TNode>();
            if (predicate != null)
                q = q.Where(predicate);
            return q;
        }


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