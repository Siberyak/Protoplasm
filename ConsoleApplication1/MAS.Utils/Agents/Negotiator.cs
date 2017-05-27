using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Core;
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
            ISatisfaction originalSatisfaction = null;
            if (Scene.Original != null)
            {
                originalSatisfaction = Scene.Original.Negotiator(Agent).Satisfaction;
            }

            //if (Agent[scene.Original]?.Request(out originalSatisfaction) != true)
            //    originalSatisfaction = null;

            Satisfaction = CreateSatisfaction(originalSatisfaction);
        }

        protected TSatisfaction Satisfaction;

        //protected TData RequestFromAgent<TData>(IScene scene)
        //{
        //    TData result;
        //    if(!Agent.Ask(new DataRequest(Scene), out result))
        //        throw new Exception();

        //    return result;
        //}

        protected abstract TSatisfaction CreateSatisfaction(ISatisfaction original);

        private IAbilitiesHolder _abilities => Agent;
        private IRequirementsHolder _requirements => Agent;

        public abstract NegotiatorState State { get; }

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

        bool IRespondent.Ask<TQuestion, TAnswer>(TQuestion question, out TAnswer answer)
        {
            return Ask(question, out answer);
        }

        protected virtual bool Ask<TQuestion, TAnswer>(TQuestion question, out TAnswer answer)
        {
            answer = default(TAnswer);
            return false;
        }

        public virtual bool Tell<TMessage>(TMessage message)
        {
            throw new System.NotImplementedException();
        }

        bool IRespondent.Request<TData>(out TData data)
        {
            //if (typeof (TData) == typeof (ISatisfaction))
            //{
            //    data = (TData)(object)Satisfaction;
            //    return true;
            //}

            //if (Request(out data))
            //    return true;

            //var question = new DataRequest(Scene);
            //return Agent.Ask(question, out data);

            return Request(out data);
        }

        protected virtual bool Request<TData>(out TData data)
        {
            data = default(TData);
            return false;
        }

        public IScene Scene { get; }
        ISatisfaction INegotiator.Satisfaction => Satisfaction;
        IAgent INegotiator.Agent => Agent;
        protected T Agent { get; }

        //public abstract IScene Variate(INegotiator respondent);
        public abstract IEnumerable<IScene> Variants(INegotiator respondent);

        void INegotiator.MergeToOriginal(INegotiator original)
        {
            var negotiator = (Negotiator<T, TSatisfaction>) original;
            negotiator.Satisfaction = Satisfaction;
            MergeToOriginal(original);
        }

        protected abstract void MergeToOriginal(INegotiator original);
    }
}