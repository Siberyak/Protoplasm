using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Utils
{
    public abstract class Negotiator<T, TSatisfaction> : INegotiator
        where T : IAgent
        where TSatisfaction : ISatisfaction
    {
        protected Negotiator(IScene scene, T agent)
        {
            Scene = scene;
            Agent = agent;
            Satisfaction = CreateSatisfaction(Agent[scene.Original]?.Satisfaction);
        }

        protected TSatisfaction Satisfaction;

        protected TData RequestFromAgent<TData>(IScene scene)
        {
            TData result;
            if(!Agent.Ask(new DataRequest(Scene), out result))
                throw new Exception();

            return result;
        }

        protected abstract TSatisfaction CreateSatisfaction(ISatisfaction original);

        public IEnumerable<int> Variate(IRequirementsHolder requirements)
        {
            return Enumerable.Empty<int>();
        }

        private IAbilitiesHolder _abilities => Agent;
        private IRequirementsHolder _requirements => Agent;

        public abstract bool IsSatisfied { get; }

        public INegotiator this[IScene scene] => Agent[scene];

        public IReadOnlyCollection<IAbility> Abilities => _abilities.Abilities;

        public IHoldersCompatibilityInfo Compatible(IRequirementsHolder requirements)
        {
            return _abilities.Compatible(requirements);
        }

        public bool Compatible(IRequirementsHolder requirements, IScene scene)
        {
            return _abilities.Compatible(requirements, scene);
        }

        public IAbility ToScene(IAbility ability)
        {
            return _abilities.ToScene(ability);
        }


        public IReadOnlyCollection<IRequirement> Requirements => _requirements.Requirements;

        public IHoldersCompatibilityInfo Compatible(IAbilitiesHolder abilities)
        {
            return _requirements.Compatible(abilities);
        }

        public bool Compatible(IAbilitiesHolder abilities, IScene scene)
        {
            return _requirements.Compatible(abilities, scene);
        }

        public IRequirement ToScene(IRequirement requirement)
        {
            return _requirements.ToScene(requirement);
        }

        public virtual bool Ask<TQuestion, TAnswer>(TQuestion question, out TAnswer answer)
        {
            throw new System.NotImplementedException();
        }

        public virtual bool Tell<TMessage>(TMessage message)
        {
            throw new System.NotImplementedException();
        }

        public virtual bool Request<TData>(out TData data)
        {
            var question = new DataRequest(Scene);
            return Agent.Ask(question, out data);
        }

        public IScene Scene { get; }
        ISatisfaction INegotiator.Satisfaction => Satisfaction;
        IAgent INegotiator.Agent => Agent;
        protected T Agent { get; }
        public abstract IScene Variate(INegotiator respondent);
    }
}