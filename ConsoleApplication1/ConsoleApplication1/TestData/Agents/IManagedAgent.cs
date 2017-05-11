using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
{
    public interface IManagedAgent : IAgent
    {
        IAgentsManager Manager { get; }
    }
}