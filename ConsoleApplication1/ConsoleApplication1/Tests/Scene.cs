using System.Collections.Generic;
using System.Linq;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData
{
    public class Scene : IScene
    {
        private Satisfaction _satisfaction;

        public Scene()
        { }

        private Scene(IScene original)
        {
            Original = original;
        }

        public IScene Original { get; }

        public ISatisfaction Satisfaction => _satisfaction ?? (_satisfaction = new Satisfaction(((Scene)Original)._satisfaction?.Value ?? 0));

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

        public IScene Branch()
        {
            return new Scene(this);
        }

        public void MergeToOriginal()
        {
        }

    }
}