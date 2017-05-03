using System;
using System.Collections.Generic;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using Protoplasm.Utils;

namespace ConsoleApplication1.TestData
{
    public class ManagedEntityAgent<TEntity> : EntityAgent<TEntity>, IManagedAgent
        where TEntity : Entity
    {
        public IAgentsManager Manager { get; }

        public ManagedEntityAgent(IAgentsManager manager, TEntity entity) : base(entity)
        {
            Manager = manager;
        }



        protected override IHoldersCompatibilityInfo AbilitiesCompatible(IRequirementsHolder requirements)
        {
            return Manager.Compatible(requirements, this);
        }

        protected override bool AbilitiesCompatible(IRequirementsHolder requirements, IScene scene)
        {
            return base.AbilitiesCompatible(requirements, scene);
        }

        protected override IHoldersCompatibilityInfo RequirementsCompatible(IAbilitiesHolder abilities)
        {
            return Manager.Compatible(this, abilities);
        }

        protected override bool RequirementsCompatible(IAbilitiesHolder abilities, IScene scene)
        {
            return base.RequirementsCompatible(abilities, scene);
        }

        public override string ToString()
        {
            return $"{GetType().DisplayName()}: [{Entity}]";
        }
    }
}