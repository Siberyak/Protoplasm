using System.Collections.Generic;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
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

        IReadOnlyCollection<IAbility> IAbilitiesHolder.Abilities => Entity.Abilities;

        IReadOnlyCollection<IRequirement> IRequirementsHolder.Requirements => Entity.Requirements;

        IHoldersCompatibilityInfo IAbilitiesHolder.Compatible(IRequirementsHolder requirements)
        {
            return AbilitiesCompatible(requirements);
        }

        protected virtual IHoldersCompatibilityInfo AbilitiesCompatible(IRequirementsHolder requirements)
        {
            return ((IAbilitiesHolder) Entity).Compatible(requirements);
        }

        bool IAbilitiesHolder.Compatible(IRequirementsHolder requirements, IScene scene)
        {
            return AbilitiesCompatible(requirements, scene);
        }

        protected virtual bool AbilitiesCompatible(IRequirementsHolder requirements, IScene scene)
        {
            return ((IAbilitiesHolder) Entity).Compatible(requirements, scene);
        }

        IHoldersCompatibilityInfo IRequirementsHolder.Compatible(IAbilitiesHolder abilities)
        {
            return RequirementsCompatible(abilities);
        }

        protected virtual IHoldersCompatibilityInfo RequirementsCompatible(IAbilitiesHolder abilities)
        {
            return ((IRequirementsHolder) Entity).Compatible(abilities);
        }

        bool IRequirementsHolder.Compatible(IAbilitiesHolder abilities, IScene scene)
        {
            return RequirementsCompatible(abilities, scene);
        }

        protected virtual bool RequirementsCompatible(IAbilitiesHolder abilities, IScene scene)
        {
            return ((IRequirementsHolder) Entity).Compatible(abilities, scene);
        }

        IAbility IAbilitiesHolder.ToScene(IAbility ability)
        {
            return ((IAbilitiesHolder) Entity).ToScene(ability);
        }

        IRequirement IRequirementsHolder.ToScene(IRequirement requirement)
        {
            return ((IRequirementsHolder)Entity).ToScene(requirement);
        }
    }
}