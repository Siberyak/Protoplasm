using System.Collections.Generic;
using System.Linq;
using MAS.Core.Contracts;

namespace MAS.Utils
{
    public abstract class Scene<TSatisfaction> : IScene
        where TSatisfaction : class, ISatisfaction
    {
        protected TSatisfaction _satisfaction;

        protected Scene()
        { }

        protected Scene(IScene original)
        {
            Original = original;
        }

        public IScene Original { get; }

        public ISatisfaction Satisfaction => _satisfaction ?? (_satisfaction = GetSatisfaction());
        protected abstract TSatisfaction GetSatisfaction();

        private readonly List<INegotiator> _negotiators = new List<INegotiator>();
        public IEnumerable<INegotiator> Negotiators
        {
            get
            {
                lock (_negotiators)
                {
                    return _negotiators.ToArray();
                }
            }
        }

        public INegotiator Negotiator(IAgent agent)
        {
            lock (_negotiators)
            {
                var negotiator = _negotiators.FirstOrDefault(x => x.Agent == agent)
                                 ?? CreateNegotiator(agent);

                return negotiator;
            }
        }

        private INegotiator CreateNegotiator(IAgent agent)
        {
            var negotiator = agent.Negotiator(this);
            _negotiators.Add(negotiator);
            return negotiator;
        }

        public abstract IScene Branch();

        public abstract void MergeToOriginal();

    }
}