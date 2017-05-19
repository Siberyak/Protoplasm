using System.Collections.Generic;
using MAS.Core.Compatibility.Contracts;

namespace MAS.Utils
{
    public abstract class ManagedAgent : Agent, IManagedAgent
    {
        protected ManagedAgent(IAgentsManager manager)
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