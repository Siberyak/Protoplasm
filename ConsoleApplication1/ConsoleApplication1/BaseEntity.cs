using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    public abstract class BaseEntity
    {
        protected BaseEntity(Guid id)
        {
            ID = id;
        }

        public Guid ID { get; }

        public abstract IReadOnlyCollection<BaseRequirement> Requirements { get; }
        public abstract IReadOnlyCollection<BaseAbility> Abilities { get; }
    }
}