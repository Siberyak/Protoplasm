using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using MAS.Core;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace ConsoleApplication1.TestData.Messaging
{
    public class MessagingTests
    {
        static Config _config = ConfigurationFactory.ParseString
    (
        @"
akka {
  actor {
    serializers {
      hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
    }
    serialization-bindings {
      ""System.Object"" = hyperion
    }
  }
}
"
    );

        private static ActorSystem ActorSystem(string name)
        {
            var system = Akka.Actor.ActorSystem.Create(name, _config);
            return system;
        }


        public static void HandlersTest()
        {
        }


        public static void AkkaTest()
        {

            var system = ActorSystem("MessagingTests-AkkaTest");
            //var actor1 = system.ActorOf<Actor1>("greeter");



            //Execute(() => Messaging<Agent>.SendRequest<int>(agent2));

            //Async(async () => await Messaging<Agent>.SendMessageAsync(new AgentGreet(agent2, "AgetnGreetMessage 2")));

            //Async(async () => Console.WriteLine("requested datetime: {0}", await agent2.RequestDateTimeAsync()));
            //Async(async () => await TestFaultAsync(agent2));

            Console.WriteLine("-= waiting =-");
            Console.WriteLine("press [enter] to continue...");
            Console.ReadLine();

        }



        static async Task TestFaultAsync(/*Agent2 reciver*/)
        {
            //Task t = Messaging<Agent>.SendRequestAsync<int>(reciver)
            //    .ContinueWith(task =>
            //    {
            //        if (!task.IsFaulted)
            //            Console.WriteLine("requested int: {0}", task.Result);
            //        else
            //            Console.WriteLine("---- request int faulted!!! -----");
            //    });

            //await ExecuteAsync(t);
            //await ExecuteAsync(Task.Run(async () => Console.WriteLine("requested int: {0}", await Messaging<Agent>.SendRequestAsync<int>(reciver))));
        }

        private static async Task ExecuteAsync(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(ex);

                Console.ForegroundColor = color;
            }
        }

        private static void Execute(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(ex);

                Console.ForegroundColor = color;
            }
        }


        static void Async(Action action)
        {
            Task.Factory.StartNew(action)
                .ContinueWith
                (
                    task =>
                    {
                        if (task.IsFaulted && task.Exception != null)
                        {
                            var color = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Red;

                            var str = string.Join(@"\n------------------------------------------------\n", task.Exception.InnerExceptions.Select(x => $"{x}"));
                            Console.WriteLine(str);

                            Console.ForegroundColor = color;
                        }
                    }
                );
        }
    }
}
