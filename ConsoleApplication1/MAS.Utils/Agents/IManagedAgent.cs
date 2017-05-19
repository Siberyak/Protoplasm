using MAS.Core.Contracts;

namespace MAS.Utils
{
    public interface IManagedAgent : IAgent
    {
        IAgentsManager Manager { get; }
    }
}