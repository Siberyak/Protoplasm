using System;
using System.Collections.Generic;

namespace Protoplasm.Utils.Graph
{
    /// <summary>
    ///     �������� ����������� �����
    /// </summary>
    public interface IMutableDataGraph : IGraph
    {
        /// <summary>
        ///     �������� ����
        /// </summary>
        IDataNode<TNodeData> Add<TNodeData>(TNodeData data);

        /// <summary>
        ///     �������� �����
        /// </summary>
        IDataEdge<TEdgeData> Add<TEdgeData>(INode from, INode to, TEdgeData data = default(TEdgeData), bool isBackreference = false);

        /// <summary>
        ///     ���������� ���� �� ������ ����
        /// </summary>
        /// <typeparam name="TNodeData">��� ������ ����</typeparam>
        /// <param name="data">������ ������ ����</param>
        /// <returns>����</returns>
        IDataNode<TNodeData> GetNode<TNodeData>(TNodeData data);

        /// <summary>
        ///     ���������� ����� �� ������ �����
        /// </summary>
        /// <typeparam name="TEdgeData">��� ������ �����</typeparam>
        /// <param name="data">������ ������ �����</param>
        /// <returns>�����</returns>
        IDataEdge<TEdgeData> GetEdge<TEdgeData>(TEdgeData data);

        /// <summary>
        ///     ������� ����
        /// </summary>
        void Remove(INode node);

        /// <summary>
        ///     ������� �����
        /// </summary>
        void Remove(IEdge edge);

        IEnumerable<T> Datas<T>(Func<T, bool> predicate = null);
        IEnumerable<IDataNode<T>> DataNodes<T>(Func<IDataNode<T>, bool> predicate = null);
        IEnumerable<IDataEdge<T>> DataEdges<T>(Func<IDataEdge<T>, bool> predicate = null);
    }
}