using System;
using System.Collections.Generic;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Utils
{
    public abstract class Entity : IAbilitiesHolder, IRequirementsHolder
    {
        public IAgent Agent { get; set; }


        public INegotiator this[IScene scene] => Agent[scene];

        public string Caption { get; }

        protected Entity(string caption)
        {
            //Guid.NewGuid()
            Caption = caption;
        }

#pragma warning disable 649
        private IReadOnlyCollection<Requirement> _requirements;
        private IReadOnlyCollection<Ability> _abilities;
#pragma warning restore 649

        public IReadOnlyCollection<IRequirement> Requirements => GetRequirements();

        protected IReadOnlyCollection<Requirement> GetRequirements()
        {
            return _requirements ?? GenerateRequirements() ?? new Requirement[0];
        }

        protected virtual IReadOnlyCollection<Requirement> GenerateRequirements()
        {
            return new Requirement[0];
        }

        public IReadOnlyCollection<IAbility> Abilities => GetAbilities();

        protected IReadOnlyCollection<Ability> GetAbilities()
        {
            return _abilities ?? GenerateAbilities() ?? new Ability[0];
        }

        protected virtual IReadOnlyCollection<Ability> GenerateAbilities()
        {
            return new Ability[0];
        }


        public override string ToString()
        {
            return Caption;
        }

        IHoldersCompatibilityInfo IAbilitiesHolder.Compatible(IRequirementsHolder requirements)
        {
            throw new NotImplementedException();
        }

        bool IAbilitiesHolder.Compatible(IRequirementsHolder requirements, IScene scene)
        {
            throw new NotImplementedException();
        }

        IHoldersCompatibilityInfo IRequirementsHolder.Compatible(IAbilitiesHolder abilities)
        {
            throw new NotImplementedException();
        }

        bool IRequirementsHolder.Compatible(IAbilitiesHolder abilities, IScene scene)
        {
            throw new NotImplementedException();
        }

        IAbility IAbilitiesHolder.ToScene(IAbility ability)
        {
            return ToScene(ability);
        }

        IRequirement IRequirementsHolder.ToScene(IRequirement requirement)
        {
            return ToScene(requirement);
        }

        protected virtual IAbility ToScene(IAbility ability)
        {
            return ((Ability)ability).ToScene();
        }

        protected virtual IRequirement ToScene(IRequirement requirement)
        {
            return ((Requirement)requirement).ToScene();
        }
    }
}