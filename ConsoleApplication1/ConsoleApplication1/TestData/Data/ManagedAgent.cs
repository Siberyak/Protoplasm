namespace ConsoleApplication1.TestData
{
    public class ManagedAgent : Agent, IManagedAgent
    {
        public ManagedAgent(IAgentsManager manager)
        {
            Manager = manager;
        }

        public IAgentsManager Manager { get; }
    }
}