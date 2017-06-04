using System;
using System.Threading;
using System.Windows.Forms;
using Application1.Data;
//using Application1.UI;
//using DevExpress.UserSkins;
//using DevExpress.Skins;
//using DevExpress.LookAndFeel;
using MAS.Core;
using MAS.Core.Contracts;
using MAS.Utils;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils;

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
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            Env env;
            IScene result;
            ISatisfaction prev;

            env = CreateEnv();
            prev = env.Satisfaction;
            if (env.SatisfyResources(out result, VariantKind.First))
            {
                //var current = env.Satisfaction;
                //Console.WriteLine($"{prev} -> {current}");
                //prev = current;
                env.Show();
            }

            Console.WriteLine();
            ConsoleExtender.Write("-= press enter to ");
            ConsoleExtender.Write("find BEST variant", ConsoleColor.Green);
            ConsoleExtender.WriteLine(" =-");
            Console.ReadLine();

            env = CreateEnv();
            prev = env.Satisfaction;
            if (env.SatisfyResources(out result, VariantKind.Best))
            {
                var current = env.Satisfaction;
                Console.WriteLine($"{prev} -> {current}");
                prev = current;
                env.Show();
            }

            Console.WriteLine();
            ConsoleExtender.Write("-= press enter to ");
            ConsoleExtender.Write("EXIT", ConsoleColor.Red);
            ConsoleExtender.WriteLine(" =-");

            Console.ReadLine();


            Console.WriteLine();
            ConsoleExtender.WriteLine("ARE YOU SHURE ???", ConsoleColor.Yellow);
            ConsoleExtender.WriteLine("-= press enter to exit =-");
            Console.ReadLine();

            return;

            if (env.SatisfyResources(out result, VariantKind.First))
            {
                var current = env.Satisfaction;
                Console.WriteLine($"{prev} -> {current}");
                prev = current;

                env.Show();
            }


            env = CreateEnv();
            if (env.SatisfyWorkItems(out result, VariantKind.First))
            {
                var current = env.Satisfaction;
                Console.WriteLine($"{prev} -> {current}");
                prev = current;

                env.Show();
            }
            if (env.SatisfyWorkItems(out result, VariantKind.First))
            {
                var current = env.Satisfaction;
                Console.WriteLine($"{prev} -> {current}");
                prev = current;

                env.Show();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //BonusSkins.Register();
            //SkinManager.EnableFormSkins();
            //UserLookAndFeel.Default.SetSkinStyle("DevExpress Style");
            //Application.Run(new Form1());
        }

        private static Env CreateEnv()
        {
            var env = new Env();

            env.ResourcesManager.CreateResource("R1: C1-7 && C2-3", Competences.New().AddKeyValue("C1", 7).AddKeyValue("C2", 3), true);
            env.ResourcesManager.CreateResource("R2: C1-3 && C2-7", Competences.New().AddKeyValue("C1", 3).AddKeyValue("C2", 7), true);
            var r3 = env.ResourcesManager.CreateResource
                (
                    "R3: C3-5",
                    Competences.New()
                        //.AddKeyValue("C1", 5)
                        //.AddKeyValue("C2", 5)
                        .AddKeyValue("C3", 5),
                    true
                );


            Interval<DateTime> start = DateTime.Today.Left().Interval(DateTime.Today.AddDays(1).Right());
            Interval<DateTime> finish = DateTime.Today.Left().Interval(DateTime.Today.AddDays(1).Right());
            Interval<TimeSpan> duration = Point<TimeSpan>.Left().Interval(TimeSpan.FromHours(8).Right());
            Interval<TimeSpan> totalDuration = Interval<TimeSpan>.Undefined;

            var cnt = 1;
            for (int i = 1; i <= 3; i++)
            {
                var wi0 = env.WorkItemsManager.CreateWorkItem($"WI({i}) #{cnt++}: C1-5 || C2-5", i);
                wi0.Add(new WorkItemBoundaryInfo(start, finish));
                var competences = Competences.New().AnyOf(Competences.New().AddKeyValue("C1", 5).AddKeyValue("C2", 5));
                wi0.Add(new ResourceRequirementInfo(null,competences,TimeSpan.Zero,duration,totalDuration));
                wi0.Add(new ResourceRequirementInfo(r3,null,TimeSpan.Zero,duration,totalDuration));

                var wi1 = env.WorkItemsManager.CreateWorkItem($"WI({i}) #{cnt++}: C1-5", i);
                wi1.Add(new WorkItemBoundaryInfo(start, finish));
                competences = Competences.New().AddKeyValue("C1", 5);
                wi1.Add(new ResourceRequirementInfo(null,competences,TimeSpan.Zero,duration,totalDuration));

                var wi2 = env.WorkItemsManager.CreateWorkItem($"WI({i}) #{cnt++}: C2-5", i);
                wi2.Add(new WorkItemBoundaryInfo(start, finish));
                competences = Competences.New().AddKeyValue("C2", 5);
                wi2.Add(new ResourceRequirementInfo(null,competences,TimeSpan.Zero,duration,totalDuration));


                var injector = env.WorkItemsManager.DependencyInjector;

                var i1 = injector
                    .Primary(wi0, WorkItemsInterval.Finish)
                    .MinDelay(TimeSpan.FromHours(8))
                    .Secondary(wi1, WorkItemsInterval.Start)
                    .Inject();

                var i2 = injector
                    .Primary(wi1, WorkItemsInterval.Start)
                    .MinDelay(TimeSpan.FromHours(-8))
                    .Secondary(wi2, WorkItemsInterval.Start)
                    .Inject();


                env.WorkItemsManager.Get(wi0);
                env.WorkItemsManager.Get(wi1);
                env.WorkItemsManager.Get(wi2);

            }
            return env;
        }
    }
}
