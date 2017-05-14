namespace lua.reader.Graph
{
    public interface IGraphEdge<T> : IGraphEdge, IDataItem<T>
    {
    }

    public interface IGraphEdge
    {

        IGraphNode From { get; }
        IGraph Graph { get; }
        IGraphNode To { get; }
    }
}