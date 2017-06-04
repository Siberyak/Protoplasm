using Application1.Data;
using MAS.Core;
using MAS.Core.Contracts;

namespace Application1
{
    public class Env
    {
        public Scene Scene { get; private set; } = new Scene();

        public ISatisfaction Satisfaction => ((IScene) Scene).Satisfaction.Snapshot();

        public ResourcesManager ResourcesManager { get; } = new ResourcesManager();
        public WorkItemsManager WorkItemsManager { get; } = new WorkItemsManager();

        ComplexedResourcesManager ComplexedResourcesManager { get; } = new ComplexedResourcesManager();

        public Env()
        {
            WorkItemsManager.AddAbilitiesManager(ComplexedResourcesManager);
            ComplexedResourcesManager.AddRequirementsManager(WorkItemsManager);

            ComplexedResourcesManager.AddAbilitiesManager(ResourcesManager);
        }

        delegate bool TrySatisfyDelegate(IScene scene, out IScene result, VariantKind kind);

        private bool Satisfy(out IScene current, VariantKind kind, TrySatisfyDelegate @delegate)
        {
            current = Scene;
            var result = false;
            var success = true;
            while (success)
            {
                success = @delegate(Scene, out current, kind);
                if (success)
                {
                    while (current.Original != Scene)
                    {
                        current.MergeToOriginal();
                        current = current.Original;
                    }
                    current.MergeToOriginal();
                }
                result = result || success;
            }
            return result;
        }

        public bool SatisfyResources(out IScene current, VariantKind kind)
        {
            current = ResourcesManager.SAbilities(Scene, kind);
            var success = current!=null && current != Scene;
            if (success)
            {
                while (current.Original != Scene)
                {
                    current.MergeToOriginal();
                    current = current.Original;
                }
                current.MergeToOriginal();
            }
            return success;

            //return Satisfy(out current, kind, ResourcesManager.TrySatisfyAbilities);
        }


        public bool SatisfyWorkItems(out IScene current, VariantKind kind)
        {
            return Satisfy(out current, kind, WorkItemsManager.TrySatisfyRequirements);
        }

        public void Show()
        {
            ResourcesManager.Show(Scene);
            WorkItemsManager.Show(Scene);

            Scene.Show();
        }
    }
}