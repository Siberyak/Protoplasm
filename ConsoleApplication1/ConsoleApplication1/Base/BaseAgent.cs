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

        /// <summary>
        /// Потребности
        /// </summary>
        public virtual IReadOnlyCollection<BaseRequirement> Requirements => BaseRequirement.Empty;


        //public abstract IEnumerable<SatisfactionVariant> GenerateVariants(IReadOnlyCollection<BaseRequirement> requirements);

        /// <summary>
        /// Проверка потенциальной возможности удовлетворить потребности другого агента
        /// </summary>
        /// <param name="requirementsAgent">агент-потребностей</param>
        /// <returns></returns>
        public virtual AgentsCompatibilityInfo Compatible(BaseAgent requirementsAgent)
        {
            return new AgentsCompatibilityInfo(requirementsAgent, this, CompatibilityInfo.Empty);
        }
    }
}