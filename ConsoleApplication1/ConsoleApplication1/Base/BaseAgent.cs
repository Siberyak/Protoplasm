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
        /// �����������
        /// </summary>
        public virtual IReadOnlyCollection<BaseRequirement> Requirements => BaseRequirement.Empty;


        //public abstract IEnumerable<SatisfactionVariant> GenerateVariants(IReadOnlyCollection<BaseRequirement> requirements);

        /// <summary>
        /// �������� ������������� ����������� ������������� ����������� ������� ������
        /// </summary>
        /// <param name="requirementsAgent">�����-������������</param>
        /// <returns></returns>
        public virtual AgentsCompatibilityInfo Compatible(BaseAgent requirementsAgent)
        {
            return new AgentsCompatibilityInfo(requirementsAgent, this, CompatibilityInfo.Empty);
        }
    }
}