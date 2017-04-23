namespace MAS.Core.Contracts
{
    public interface IEntityAgent<T> : IIdentifiedAgent
    {
        T Entity { get; }
    }
}