using MAS.Core.Contracts;
using MAS.Utils;

namespace ConsoleApplication1.TestData
{
    public class Scene : Scene<Satisfaction>
    {
        public Scene()
        {
        }

        private Scene(IScene original) : base(original)
        {
        }

        protected override Satisfaction GetSatisfaction()
        {
            var original= ((Scene) Original)?._satisfaction;
            return new Satisfaction(original?.Value ?? 0);
        }

        public override IScene Branch()
        {
            return new Scene(this);
        }

        public override void MergeToOriginal()
        {
        }
    }
}