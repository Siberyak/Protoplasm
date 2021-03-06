using System.Collections.Generic;
using System.Linq;
using MAS.Core.Contracts;

namespace MAS.Utils
{
    public abstract class Scene<TSatisfaction> : IScene
        where TSatisfaction : class, ISatisfaction
    {
        protected virtual TSatisfaction Satisfaction { get; set; }

        protected Scene()
        { }

        protected Scene(Scene<TSatisfaction> original)
        {
            Original = original;
        }

        protected Scene<TSatisfaction> Original;

        IScene IScene.Original => Original;

        ISatisfaction IScene.Satisfaction => Satisfaction;// ?? (Satisfaction = GetSatisfaction());
        //protected abstract TSatisfaction GetSatisfaction();

        protected readonly List<INegotiator> Negotiators = new List<INegotiator>();

        IEnumerable<INegotiator> IScene.Negotiators
        {
            get
            {
                lock (Negotiators)
                {
                    return Negotiators.ToArray();
                }
            }
        }

        public INegotiator Negotiator(IAgent agent)
        {
            lock (Negotiators)
            {
                var negotiator = Negotiators.FirstOrDefault(x => x.Agent == agent)
                                 ?? CreateNegotiator(agent);

                return negotiator;
            }
        }

        private INegotiator CreateNegotiator(IAgent agent)
        {
            var negotiator = agent.Negotiator(this);
            lock (Negotiators)
            {
                Negotiators.Add(negotiator);
            }
            return negotiator;
        }

        public abstract IScene Branch();

        //public abstract void MergeToOriginal();

        public virtual void MergeToOriginal()
        {
            if (Original == null)
                return;

            lock (Original)
            {
                lock (Original.Negotiators)
                {
                    lock (Negotiators)
                    {
                        foreach (var negotiator in Negotiators.ToArray())
                        {
                            Original.ReplaceNegotiator(negotiator);
                        }
                    }
                }
            }
        }

        private void ReplaceNegotiator(INegotiator negotiator)
        {
            lock (Negotiators)
            {
                var original = Negotiators.First(x => x.Agent == negotiator.Agent);
                negotiator.MergeToOriginal(original);
                //((IFlatableSatisfaction) negotiator.Satisfaction)?.Flat();
            }

        }
    }
}