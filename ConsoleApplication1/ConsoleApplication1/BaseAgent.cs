using System.Collections.Generic;

namespace ConsoleApplication1
{
    public abstract class BaseAgent
    {
        public void Initialize()
        {
            RegisterBehaviors();
        }

        protected abstract void RegisterBehaviors();

        //public abstract IReadOnlyCollection<BaseAbility> Abilities { get; }

        public virtual IReadOnlyCollection<BaseRequirement> Requirements => BaseRequirement.Empty;


        //public abstract IEnumerable<SatisfactionVariant> GenerateVariants(IReadOnlyCollection<BaseRequirement> requirements);

        public virtual ConformableInfo[] ConformableFor(BaseAgent requirementsAgent)
        {
            return ConformableInfo.Empty;
        }
    }
}