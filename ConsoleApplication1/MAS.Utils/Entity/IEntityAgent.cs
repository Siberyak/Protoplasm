using MAS.Core.Contracts;

namespace MAS.Utils
{
    public interface IEntityAgent<T> : IAgent
        where T : Entity
    {
        T Entity { get; }
    }
}