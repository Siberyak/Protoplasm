using System.Linq;
using MAS.Core.Contracts;
using MAS.Utils;

namespace Application1.Data
{
    public class Scene : Scene<Satisfaction>
    {
        private static int _idSeq = 0;

        private int _id = _idSeq++;
        public Scene()
        {
            Satisfaction = new Satisfaction(0);
        }

        private Scene(Scene original) : base(original)
        {
            var originalValue = ((Satisfaction)((Scene)Original)?.Satisfaction.Snapshot()).Value;

            Satisfaction = new Satisfaction(originalValue);
        }

        public override string ToString()
        {
            return $"#{_id}: {Satisfaction}";
        }

        protected override Satisfaction Satisfaction
        {
            get
            {

                lock (Negotiators)
                {
                    var satisfactions = Negotiators.Select(x => x.Satisfaction).Cast<Satisfaction>().Where(x => x != null);
                    base.Satisfaction.Δ = satisfactions.Sum(x => x.Δ);
                    
                }
                return base.Satisfaction;
            }
            set { base.Satisfaction = value; }
        }

        //protected override Satisfaction Satisfaction
        //{
        //    get
        //    {
        //        if (base.Satisfaction == null)
        //            return null;

        //        var satisfactions = Negotiators.Select(x => x.Satisfaction).Cast<Satisfaction>().Where(x => x != null);
        //        base.Satisfaction.Δ = satisfactions.Sum(x => x.Δ);

        //        return base.Satisfaction;
        //    }
        //    set { base.Satisfaction = value; }
        //}

        //protected Satisfaction GetSatisfaction()
        //{
        //    var original = ((Scene)Original)?.Satisfaction;

        //    var satisfactions = Negotiators.Select(x => x.Satisfaction).Cast<Satisfaction>().Where(x => x != null);
        //    var δ = satisfactions.Sum(x => x.Δ);

        //    return new Satisfaction(original?.Value ?? 0) {Δ = (original?.Δ ?? 0) + δ };
        //}

        public override IScene Branch()
        {
            return new Scene(this);
        }

        public void Show()
        {
            
        }
    }
}