using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Core
{
    public abstract class ManagedEntityAgent<T> : EntityAgent<T> , IManagedAgent
        where T : BaseEntity
    {
        public IAgentsManager Manager { get; }

        protected ManagedEntityAgent(IAgentsManager manager, T entity) : base(entity)
        {
            Manager = manager;
        }

        protected override ICompatibilitiesAgent CompatibilitiesAgent => Manager.CompatibilitiesAgent;
    }
}