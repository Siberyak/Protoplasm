using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MAS.Core
{
    public abstract class EntityAgent<T> : BaseAgent
        where T : BaseEntity
    {
        protected EntityAgent(T entity)
        {
            Entity = entity;
        }

        protected T Entity { get; }

        public override IReadOnlyCollection<BaseRequirement> Requirements => Entity.Requirements;

        //public override AgentsCompatibilityInfo Compatible(BaseAgent requirementsAgent)
        //{
        //    var result = requirementsAgent.Requirements.Compatibility(Entity.Abilities);

        //    return new AgentsCompatibilityInfo(requirementsAgent, this, result);
        //}

        protected void LifeCircle()
        {
            if (Satisfied())
            {
                return;
            }

            foreach (var variant in GetStisfactionVariants())
            {
                if(!Estimate(variant))
                    continue;
            }
        }

        private bool Satisfied()
        {
            return true;
        }

        protected IEnumerable<SatisfactionVariant> GetStisfactionVariants()
        {
            return Enumerable.Empty<SatisfactionVariant>();
        }

        public bool Estimate(SatisfactionVariant variant)
        {
            return true;
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {Entity}";
        }
    }

    public class SatisfactionVariant
    {
    }
}