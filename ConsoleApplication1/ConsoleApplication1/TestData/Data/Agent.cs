using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
{
    public abstract class Agent : IAgent
    {
        public virtual bool Ask<TQuestion, TAnswer>(TQuestion question, out TAnswer answer)
        {
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

        public virtual INegotiator this[IScene scene] => (scene as Scene)?.Negotiator(this);


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
        public INegotiator Negotiator(IScene scene)
        {
            return Negotiator((Scene) scene);
        }

        protected abstract INegotiator Negotiator(Scene scene);

    }
}