using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
{
    public interface IEntityAgent<T> : IAgent, IAbilitiesHolder, IRequirementsHolder
        where T : Entity
    {
        T Entity { get; }
    }
}