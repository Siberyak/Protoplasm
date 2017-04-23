using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Core;

namespace ConsoleApplication1.TestData
{
    public abstract class Entity : BaseEntity
    {
        public string Caption { get; }

        protected Entity(string caption)
            : base(Guid.NewGuid())
        {
            Caption = caption;
        }

#pragma warning disable 649
        private IReadOnlyCollection<Requirement> _requirements;
        private IReadOnlyCollection<Ability> _abilities;
#pragma warning restore 649

        public override IReadOnlyCollection<BaseRequirement> Requirements => GetRequirements();

        protected IReadOnlyCollection<Requirement> GetRequirements()
        {
            return _requirements ?? GenerateRequirements() ?? new Requirement[0];
        }

        protected virtual IReadOnlyCollection<Requirement> GenerateRequirements()
        {
            return new Requirement[0];
        }

        public override IReadOnlyCollection<BaseAbility> Abilities => GetAbilities();

        protected IReadOnlyCollection<BaseAbility> GetAbilities()
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
    }
}