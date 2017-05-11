using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
{
    public abstract class Negotiator<T> : INegotiator
        where T : IAgent
    {
        protected Negotiator(IScene scene, T agent)
        {
            Scene = scene;
            Agent = agent;
            Satisfaction = Agent[scene.Original]?.Satisfaction ?? new Satisfaction(0);
        }

        public IEnumerable<int> Variate(IRequirementsHolder requirements)
        {
            return Enumerable.Empty<int>();
        }



        private IAbilitiesHolder _abilities => Agent;
        private IRequirementsHolder _requirements => Agent;

        public INegotiator this[IScene scene]
        {
            get { return Agent[scene]; }
        }

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

        public bool Ask<TQuestion, TAnswer>(TQuestion question, out TAnswer answer)
        {
            throw new System.NotImplementedException();
        }

        public bool Tell<TMessage>(TMessage message)
        {
            throw new System.NotImplementedException();
        }

        public bool Request<TData>(out TData data)
        {
            throw new System.NotImplementedException();
        }

        public IScene Scene { get; }
        public ISatisfaction Satisfaction { get; }
        IAgent INegotiator.Agent => Agent;
        protected T Agent { get; }
        public abstract IScene Variate(INegotiator abilities);
    }
}