namespace MAS.Core.Contracts
{
    public interface IManagedAgent : IAgent
    {
        IAgentsManager Manager { get; }
    }
}