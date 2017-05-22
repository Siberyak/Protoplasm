using System;
using System.Windows.Forms;
using Application1.Data;
using Application1.UI;
using DevExpress.UserSkins;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using MAS.Core;
using MAS.Core.Contracts;
using MAS.Utils;

namespace Application1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Env env;
            IScene result;

            env = CreateEnv();
            if (env.SatisfyResources(out result, VariantKind.First))
                env.Show(result);

            env = CreateEnv();
            if (env.SatisfyWorkItems(out result, VariantKind.First))
                env.Show(result);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            BonusSkins.Register();
            SkinManager.EnableFormSkins();
            UserLookAndFeel.Default.SetSkinStyle("DevExpress Style");
            Application.Run(new Form1());
        }

        private static Env CreateEnv()
        {
            var env = new Env();

            env.ResourcesManager.CreateResource("R1", Competences.New().AddKeyValue("C1", 7).AddKeyValue("C2", 3));
            env.ResourcesManager.CreateResource("R2", Competences.New().AddKeyValue("C1", 3).AddKeyValue("C2", 7));

            env.WorkItemsManager.CreateWorkItemAgent("WI: C1-5 || C2-5", Competences.New().AnyOf(Competences.New().AddKeyValue("C1", 5).AddKeyValue("C2", 5)));
            env.WorkItemsManager.CreateWorkItemAgent("WI: C1-5", Competences.New().AddKeyValue("C1", 5));
            env.WorkItemsManager.CreateWorkItemAgent("WI: C2-5", Competences.New().AddKeyValue("C1", 5));
            return env;
        }
    }


    public class Env
    {
        public IScene Scene { get; private set; } = new Scene();

        public ResourcesManager ResourcesManager { get; } = new ResourcesManager();
        public WorkItemsManager WorkItemsManager { get; } = new WorkItemsManager();

        public Env()
        {
            WorkItemsManager.AddAbilitiesManager(ResourcesManager);
            ResourcesManager.AddRequirementsManager(WorkItemsManager);
        }

        public bool SatisfyResources(out IScene result, VariantKind kind)
        {
            var success = ResourcesManager.TrySatisfyAbilities(Scene, out result, kind);
            if (success)
                result.MergeToOriginal();
            return success;
        }

        public bool SatisfyWorkItems(out IScene result, VariantKind kind)
        {
            return WorkItemsManager.TrySatisfyRequirements(Scene, out result, kind);
        }

        public void Show(IScene scene)
        {
            
        }
    }
}
