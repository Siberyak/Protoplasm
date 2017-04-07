using System;
using System.Collections.Generic;

namespace Protoplasm.Utils.Graph
{
    /// <summary>
    ///     Описание изменяемого графа
    /// </summary>
    public interface IMutableDataGraph : IGraph
    {
        /// <summary>
        ///     Добавить узел
        /// </summary>
        IDataNode<TNodeData> Add<TNodeData>(TNodeData data);

        /// <summary>
        ///     Добавить связь
        /// </summary>
        IDataEdge<TEdgeData> Add<TEdgeData>(INode from, INode to, TEdgeData data = default(TEdgeData), bool isBackreference = false);

        /// <summary>
        ///     Возвращает узел по данным узла
        /// </summary>
        /// <typeparam name="TNodeData">Тип данных узла</typeparam>
        /// <param name="data">Объект данных узла</param>
        /// <returns>Узел</returns>
        IDataNode<TNodeData> GetNode<TNodeData>(TNodeData data);

        /// <summary>
        ///     Возвращает связь по данным связи
        /// </summary>
        /// <typeparam name="TEdgeData">Тип данных связи</typeparam>
        /// <param name="data">Объект данных связи</param>
        /// <returns>Связь</returns>
        IDataEdge<TEdgeData> GetEdge<TEdgeData>(TEdgeData data);

        /// <summary>
        ///     Удалить узел
        /// </summary>
        void Remove(INode node);

        /// <summary>
        ///     Удалить связь
        /// </summary>
        void Remove(IEdge edge);

        IEnumerable<T> Datas<T>(Func<T, bool> predicate = null);
        IEnumerable<IDataNode<T>> DataNodes<T>(Func<IDataNode<T>, bool> predicate = null);
        IEnumerable<IDataEdge<T>> DataEdges<T>(Func<IDataEdge<T>, bool> predicate = null);
    }
}