using System;
using System.Collections.Generic;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using Protoplasm.Utils;

namespace ConsoleApplication1.TestData
{
    public abstract class ManagedEntityAgent<TEntity> : EntityAgent<TEntity>, IManagedAgent
        where TEntity : Entity
    {
        public IAgentsManager Manager { get; }

        public ManagedEntityAgent(IAgentsManager manager, TEntity entity) : base(entity)
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

        public override bool Compatible(IAbilitiesHolder abilities, IScene scene)
        {
            return base.Compatible(abilities, scene);
        }

        bool IRequirementsHolder.Compatible(IAbilitiesHolder abilities, IScene scene)
        {
            return Compatible(abilities, scene);
        }

        bool IAbilitiesHolder.Compatible(IRequirementsHolder requirements, IScene scene)
        {
            return Compatible(requirements, scene);
        }

        public override string ToString()
        {
            return $"{GetType().DisplayName()}: [{Entity}]";
        }

        public override IEnumerable<IHoldersCompatibilityInfo> CompatibilityInfos()
        {
            return Manager[this];
        }
    }
}