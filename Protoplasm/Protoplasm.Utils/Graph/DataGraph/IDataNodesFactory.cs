namespace Protoplasm.Utils.Graph
{
    /// <summary>
    ///     �������� ������� ��� �������� ����
    /// </summary>
    /// <typeparam name="TNodeData">��� ������ ����</typeparam>
    public interface IDataNodesFactory<TNodeData>
    {
        /// <summary>
        ///     ������� ����
        /// </summary>
        /// >
        IDataNode<TNodeData> Create(TNodeData data);
    }
}