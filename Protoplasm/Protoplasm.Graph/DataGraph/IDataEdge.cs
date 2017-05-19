namespace Protoplasm.Graph
{
    /// <summary>
    ///     Описание связи
    /// </summary>
    /// <typeparam name="TEdgeData">Тип данных связи</typeparam>
    public interface IDataEdge<out TEdgeData> : IDataEdge
    {
        /// <summary>
        ///     Объект данных связи
        /// </summary>
        TEdgeData Data { get; }
    }

    public interface IDataEdge : IEdge
    {
        new IMutableDataGraph Graph { get; }
        object EdgeData();
    }

    public interface IDataForEdge
    {
        IDataEdge Edge { get; }

        void SetEdge(IDataEdge dataEdge);
    }

}