using System.Collections.Generic;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using Protoplasm.Utils;

namespace MAS.Utils
{
    public abstract class EntityAgent<T> : Agent, IEntityAgent<T>
        where T : Entity
    {
        protected EntityAgent(T entity)
        {
            Entity = entity;
        }

        public T Entity { get; }

        public bool Equals(IAbilitiesHolder other)
        {
            return ReferenceEquals(this, other);
        }

        public bool Equals(IRequirementsHolder other)
        {
            return ReferenceEquals(this, other);
        }
        public override string ToString()
        {
            return $"{GetType().DisplayName()}: [{Entity}]";
        }

        public override IReadOnlyCollection<IRequirement> Requirements => Entity.Requirements;
        public override IReadOnlyCollection<IAbility> Abilities =>  Entity.Abilities;

        public override IHoldersCompatibilityInfo Compatible(IRequirementsHolder requirements)
        {
            return ((IAbilitiesHolder)Entity).Compatible(requirements);
        }

        public override bool Compatible(IRequirementsHolder requirements, IScene scene)
        {
            return ((IAbilitiesHolder)Entity).Compatible(requirements, scene);
        }


        public override IHoldersCompatibilityInfo Compatible(IAbilitiesHolder abilities)
        {
            return ((IRequirementsHolder)Entity).Compatible(abilities);
        }

        public override bool Compatible(IAbilitiesHolder abilities, IScene scene)
        {
            return ((IRequirementsHolder)Entity).Compatible(abilities, scene);
        }

        public override IAbility ToScene(IAbility ability)
        {
            return ((IAbilitiesHolder) Entity).ToScene(ability);
        }

        public override IRequirement ToScene(IRequirement requirement)
        {
            return ((IRequirementsHolder)Entity).ToScene(requirement);
        }
    }
}