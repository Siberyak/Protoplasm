using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Core
{
    public abstract partial class BaseAgent : IAgent, IMessanger
    {
        protected abstract ICompatibilitiesAgent CompatibilitiesAgent { get; }

        ICompatibilitiesAgent IAgent.CompatibilitiesAgent => CompatibilitiesAgent;

        public void Initialize()
        {
            InitPersonalHandlers(_personalHandlers);
        }


        /// <summary>
        /// Потребности
        /// </summary>
        public virtual IReadOnlyCollection<BaseRequirement> Requirements => BaseRequirement.Empty;

        /// <summary>
        /// Проверка потенциальной возможности удовлетворить потребности другого агента
        /// </summary>
        /// <param name="requirementsAgent">агент-потребностей</param>
        /// <returns></returns>
        public virtual IAbilitiesCompatibilityInfo Compatible(IAgent requirementsAgent)
        {
            return null;

            //var comaptibilitiesAgent = GetComaptibilitiesAgent();
            //IAbilitiesCompatibilityInfo result;

            //comaptibilitiesAgent.RequestAbilitiesCompatibilities(this, )
            //comaptibilitiesAgent.TryGetAbilitiesCompatibilities(this, requirementsAgent, out result);

            ////IAbilitiesCompatibilityInfo result;
            ////return requirementsAgent.AbilitiesCompatibilities.TryGetValue(requirementsAgent, out result)
            ////    ? result
            ////    : new HoldersCompatibilityInfo(requirementsAgent, this, CompatibilityInfo.Empty);
        }


        //protected readonly ConcurrentDictionary<BaseAgent, IRequiremetsCompatibilityInfo> RequiremetsCompatibilities
        //    = new ConcurrentDictionary<BaseAgent, IRequiremetsCompatibilityInfo>();

        //protected readonly ConcurrentDictionary<BaseAgent, IAbilitiesCompatibilityInfo> AbilitiesCompatibilities
        //    = new ConcurrentDictionary<BaseAgent, IAbilitiesCompatibilityInfo>();

        protected internal void AddCompatibilityInfo(IAbilitiesCompatibilityInfo info)
        {
            //AbilitiesCompatibilities.TryAdd(info.Holder, info);
        }
        protected internal void AddCompatibilityInfo(IRequirementsCompatibilityInfo info)
        {
            //RequiremetsCompatibilities.TryAdd(info.Holder, info);
        }

        bool IEquatable<IAgent>.Equals(IAgent other)
        {
            return other != null && IsEquals(other);
        }

        protected abstract bool IsEquals(IAgent other);
    }

    



}