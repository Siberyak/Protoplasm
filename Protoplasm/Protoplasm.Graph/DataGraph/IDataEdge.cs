namespace Protoplasm.Graph
{
    /// <summary>
    ///     �������� �����
    /// </summary>
    /// <typeparam name="TEdgeData">��� ������ �����</typeparam>
    public interface IDataEdge<out TEdgeData> : IDataEdge
    {
        /// <summary>
        ///     ������ ������ �����
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