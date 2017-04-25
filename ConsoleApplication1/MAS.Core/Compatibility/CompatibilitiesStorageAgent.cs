using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Core.Compatibility
{
    public class CompatibilitiesStorageAgent : IdentifiedAgent, ICompatibilitiesAgent
    {
        protected override ICompatibilitiesAgent CompatibilitiesAgent => this;

        private readonly ConcurrentDictionary<IRequirementsHolder, ConcurrentDictionary<IAbilitiesHolder, IAbilitiesCompatibilityInfo>> _abilitiesCompatibilities
            = new ConcurrentDictionary<IRequirementsHolder, ConcurrentDictionary<IAbilitiesHolder, IAbilitiesCompatibilityInfo>>();

        private readonly ConcurrentDictionary<IAbilitiesHolder, ConcurrentDictionary<IRequirementsHolder, IRequirementsCompatibilityInfo>> _requiremetsCompatibilities
            = new ConcurrentDictionary<IAbilitiesHolder, ConcurrentDictionary<IRequirementsHolder, IRequirementsCompatibilityInfo>>();


        protected override void InitPersonalHandlers(HandlersStorage handlers)
        {
            handlers.GetBuilder<CompatibilitiesStorageAgent>()
                .Told<string>((x, s) => s == "start", (x, s) => Start())
                .Told<string>((x, s) => Start())
                .Requested(x => DateTime.Now)
                .Asked<string, Guid>((x, s) => s == "ID", (x, s) => ID);
        }



        static readonly HandlersStorage Handlers = InitStaticHandlers();

        private static HandlersStorage InitStaticHandlers()
        {
            var handlers = new HandlersStorage();

            handlers.GetBuilder<CompatibilitiesStorageAgent>()
                .Asked<Guid, Guid>((a, q) => a.ID != q, (x, q) => x.ID)
                .Requested(x => x.Requirements)
                ;

            return handlers;
        }

        protected override HandlersStorage GetStaticHandlers()
        {
            return Handlers;
        }

        private void Start()
        {
            
        }
        
        

        void ICompatibilitiesAgent.Add(IAbilitiesCompatibilityInfo abilitiesCompatibilityInfo)
        {
            throw new NotImplementedException();
        }

        void ICompatibilitiesAgent.Add(IRequirementsCompatibilityInfo requirementsCompatibilityInfo)
        {
            throw new NotImplementedException();
        }

        //internal void RegisterAgent(IAgent agent)
        //{
        //    _abilitiesCompatibilities.TryAdd(agent, new ConcurrentDictionary<IAgent, IAbilitiesCompatibilityInfo>());
        //    _requiremetsCompatibilities.TryAdd(agent, new ConcurrentDictionary<IAgent, IRequiremetsCompatibilityInfo>());
        //}

        //public IEnumerable<IAbilitiesCompatibilityInfo> AbilitiesCompatibilities(IAgent agent)
        //{
        //    return Get(agent, _abilitiesCompatibilities);
        //}
        //public IEnumerable<IRequiremetsCompatibilityInfo> RequirementsCompatibilities(IAgent agent)
        //{
        //    return Get(agent, _requiremetsCompatibilities);
        //}

        private IEnumerable<T> Get<T1,T2,T>(T1 agent, IDictionary<T1, ConcurrentDictionary<T2, T>> infos)
        {
            ConcurrentDictionary<T2, T> result;
            if (!infos.TryGetValue(agent, out result))
            {
                //RegisterAgent(agent);

                if (!infos.TryGetValue(agent, out result))
                {
                    result = null;
                }
            }

            return result?.Values ?? Enumerable.Empty<T>();
        }
    }
}