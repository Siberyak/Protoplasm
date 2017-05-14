namespace Factorio.Lua.Reader
{
    abstract class NodeData<T> : VirtualTreeListData<T>
        where T : IStoragedData
    {
        public NodeData(T data) : base(data.Storage, data)
        {
        }
    }
}