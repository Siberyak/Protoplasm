using System;
using MAS.Core.Contracts;
using Protoplasm.Utils;

namespace MAS.Core
{
    public abstract class IdentifiedAgent : BaseAgent, IIdentifiedAgent
    {
        public Guid ID { get; } = Guid.NewGuid();

        protected override bool IsEquals(IAgent other)
        {
            return this.AreEquals(other, (a, b) => a.ID == b.ID);
        }
    }
}