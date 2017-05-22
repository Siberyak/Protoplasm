using System;
using System.Collections.Generic;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Utils
{
    public abstract class Agent : IAgent
    {

        public virtual bool Ask<TQuestion, TAnswer>(TQuestion question, out TAnswer answer)
        {
            var request = question as IDataRequest;
            if (request != null)
            {
                answer = default(TAnswer);
                var byNegotiator = request.Scene.Original?.Negotiator(this).Request(out answer);
                return byNegotiator == true || Request(out answer);
            }
            
            throw new NotImplementedException();
        }

        public virtual bool Tell<TMessage>(TMessage message)
        {
            throw new NotImplementedException();
        }

        public virtual bool Request<TData>(out TData data)
        {
            throw new NotImplementedException();
        }

        public virtual INegotiator this[IScene scene] => scene?.Negotiator(this);


        public abstract IReadOnlyCollection<IRequirement> Requirements { get; }
        public abstract IHoldersCompatibilityInfo Compatible(IAbilitiesHolder abilities);
        public abstract bool Compatible(IAbilitiesHolder abilities, IScene scene);
        public abstract IRequirement ToScene(IRequirement requirement);
        public abstract IReadOnlyCollection<IAbility> Abilities { get; }
        public abstract IHoldersCompatibilityInfo Compatible(IRequirementsHolder requirements);
        public abstract bool Compatible(IRequirementsHolder requirements, IScene scene);
        public abstract IAbility ToScene(IAbility ability);
        public abstract void Initialize();
        public abstract IEnumerable<IHoldersCompatibilityInfo> CompatibilityInfos();

        INegotiator IAgent.Negotiator(IScene scene)
        {
            return Negotiator(scene);
        }

        protected abstract INegotiator Negotiator(IScene scene);

    }
}