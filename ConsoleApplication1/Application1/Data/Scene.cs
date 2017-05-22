using System.Linq;
using MAS.Core.Contracts;
using MAS.Utils;

namespace Application1.Data
{
    public class Scene : Scene<Satisfaction>
    {
        public Scene()
        {
            
        }

        private Scene(IScene original) : base(original)
        {
        }

        protected override Satisfaction Satisfaction
        {
            get
            {
                if (base.Satisfaction == null)
                    return null;

                var satisfactions = Negotiators.Select(x => x.Satisfaction).Cast<Satisfaction>().Where(x => x != null);
                base.Satisfaction.Δ = satisfactions.Sum(x => x.Δ);

                return base.Satisfaction;
            }
            set { base.Satisfaction = value; }
        }

        protected override Satisfaction GetSatisfaction()
        {
            var original = ((Scene)Original)?.Satisfaction;
            return new Satisfaction(original?.Value ?? 0);
        }

        public override IScene Branch()
        {
            return new Scene(this);
        }

        public override void MergeToOriginal()
        {
            if(Original == null)
                return;
            Original.Negotiators;
            foreach (var negotiator in Negotiators.ToArray())
            {
                
            }
        }
    }
}