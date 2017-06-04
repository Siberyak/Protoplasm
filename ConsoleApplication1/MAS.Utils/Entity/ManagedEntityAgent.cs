using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using Protoplasm.Utils;

namespace MAS.Utils
{
    public abstract class ManagedEntityAgent<TEntity> : EntityAgent<TEntity>, IManagedAgent
        where TEntity : Entity
    {
        IAgentsManager IManagedAgent.Manager => Manager;

        protected IAgentsManager Manager { get; }

        protected ManagedEntityAgent(IAgentsManager manager, TEntity entity) : base(entity)
        {
            Manager = manager;
        }

        public override IHoldersCompatibilityInfo Compatible(IRequirementsHolder requirements)
        {
            return Manager.Compatible(this, requirements);
        }

        public override IHoldersCompatibilityInfo Compatible(IAbilitiesHolder abilities)
        {
            return Manager.Compatible(abilities, this);
        }

        public override bool Compatible(IRequirementsHolder requirements, IScene scene)
        {
            var info = Manager.Compatible(this, requirements);
            return base.Compatible(requirements, scene);
        }

        bool IRequirementsHolder.Compatible(IAbilitiesHolder abilities, IScene scene)
        {
            return Compatible(abilities, scene);
        }

        bool IAbilitiesHolder.Compatible(IRequirementsHolder requirements, IScene scene)
        {
            return Compatible(requirements, scene);
        }

        public override IEnumerable<IHoldersCompatibilityInfo> CompatibilityInfos()
        {
            return Manager.Where(x => x.AbilitiesHolder == this || x.RequiremenetsHolder == this);
        }
    }
}