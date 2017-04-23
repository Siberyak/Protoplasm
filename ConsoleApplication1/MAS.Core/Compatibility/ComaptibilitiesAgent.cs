using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MAS.Core
{
    public class ComaptibilitiesAgent : BaseAgent
    {
        private readonly ConcurrentDictionary<BaseAgent, ConcurrentDictionary<BaseAgent, IAbilitiesCompatibilityInfo>> _abilitiesCompatibilities
            = new ConcurrentDictionary<BaseAgent, ConcurrentDictionary<BaseAgent, IAbilitiesCompatibilityInfo>>();

        private readonly ConcurrentDictionary<BaseAgent, ConcurrentDictionary<BaseAgent, IRequiremetsCompatibilityInfo>> _requiremetsCompatibilities
            = new ConcurrentDictionary<BaseAgent, ConcurrentDictionary<BaseAgent, IRequiremetsCompatibilityInfo>>();

        protected override void RegisterBehaviors()
        {

        }

        internal void RegisterAgent(BaseAgent agent)
        {
            _abilitiesCompatibilities.TryAdd(agent, new ConcurrentDictionary<BaseAgent, IAbilitiesCompatibilityInfo>());
            _requiremetsCompatibilities.TryAdd(agent, new ConcurrentDictionary<BaseAgent, IRequiremetsCompatibilityInfo>());
        }

        public IEnumerable<IAbilitiesCompatibilityInfo> AbilitiesCompatibilities(BaseAgent agent)
        {
            return Get(agent, _abilitiesCompatibilities);
        }
        public IEnumerable<IRequiremetsCompatibilityInfo> RequirementsCompatibilities(BaseAgent agent)
        {
            return Get(agent, _requiremetsCompatibilities);
        }

        private IEnumerable<T> Get<T>(BaseAgent agent, IDictionary<BaseAgent, ConcurrentDictionary<BaseAgent, T>> infos)
        {
            ConcurrentDictionary<BaseAgent, T> result;
            if (!infos.TryGetValue(agent, out result))
            {
                RegisterAgent(agent);

                if (!infos.TryGetValue(agent, out result))
                {
                    result = null;
                }
            }

            return result?.Values ?? Enumerable.Empty<T>();
        }
    }
}