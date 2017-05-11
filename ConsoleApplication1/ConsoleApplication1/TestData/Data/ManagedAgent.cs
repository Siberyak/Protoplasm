using System.Collections.Generic;
using MAS.Core.Compatibility.Contracts;

namespace ConsoleApplication1.TestData
{
    public abstract class ManagedAgent : Agent, IManagedAgent
    {
        public ManagedAgent(IAgentsManager manager)
        {
            Manager = manager;
        }

        public IAgentsManager Manager { get; }

        public override IEnumerable<IHoldersCompatibilityInfo> CompatibilityInfos()
        {
            return Manager[this];
        }
    }
}