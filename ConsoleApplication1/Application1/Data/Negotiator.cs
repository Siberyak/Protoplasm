using MAS.Core.Contracts;
using MAS.Utils;

namespace Application1.Data
{
    public abstract class Negotiator<T> : Negotiator<T, Satisfaction>
        where T : IAgent
    {
        protected Negotiator(IScene scene, T agent) : this(scene, agent, new Satisfaction(0))
        {
        }

        protected Negotiator(IScene scene, T agent, Satisfaction satisfaction) : base(scene, agent)
        {
            Satisfaction = satisfaction ?? Satisfaction;
        }

        protected override Satisfaction CreateSatisfaction(ISatisfaction original)
        {
            var satisfaction = original as Satisfaction;
            return new Satisfaction(satisfaction?.Value ?? 0);
        }
    }
}