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
        public static void AkkaTest()
        {
            var config = ConfigurationFactory.ParseString
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


            Messaging<IActorRef>.SendDelegate = SendMessage;

            ActorSystem system = ActorSystem.Create("MessagingTests-AkkaTest", config);
            var actor1 = system.ActorOf<Actor1>("greeter");


            var message = new Greet(actor1, "Message 1");
            Messaging<IActorRef>.SendMessage(message);

            actor1.Tell(new Greet(actor1, "Message 2"));

            var agent2 = new Agent2(system);


            Messaging<Agent>.SendDelegate = SendMessage;
            Messaging<Agent>.SendAndReciveDelegate = SendAndRecive;
            Messaging<Agent>.SendAsyncDelegate = x => Task.Run(() => SendMessage(x));
            Messaging<Agent>.SendAndReciveAsyncDelegate = SendAndReciveAsync;

            Messaging<Agent>.SendMessage(new AgentGreet(agent2, "AgetnGreetMessage 1"));

            Execute(()=> Messaging<Agent>.SendRequest<int>(agent2));

            Async(async () => await Messaging<Agent>.SendMessageAsync(new AgentGreet(agent2, "AgetnGreetMessage 2")));

            Async(async () => Console.WriteLine("requested datetime: {0}", await agent2.RequestDateTimeAsync()));
            Async(async () => await TestFaultAsync(agent2));

            Console.WriteLine("-= waiting =-");
            Console.WriteLine("press [enter] to continue...");
            Console.ReadLine();

        }

        private static Messaging<Agent>.IResponse SendAndRecive(Messaging<Agent>.IRequest request)
        {
            return SendAndReciveAsync(request).Result;
        }



        static async Task TestFaultAsync(Agent2 reciver)
        {
            Task t = Messaging<Agent>.SendRequestAsync<int>(reciver)
                .ContinueWith(task =>
                {
                    if (!task.IsFaulted)
                        Console.WriteLine("requested int: {0}", task.Result);
                    else
                        Console.WriteLine("---- request int faulted!!! -----");
                });
                
            await ExecuteAsync(t);
            await ExecuteAsync(Task.Run(async () => Console.WriteLine("requested int: {0}", await Messaging<Agent>.SendRequestAsync<int>(reciver))));
        }

        private static async Task ExecuteAsync(Task task)
        {
            try
            {
                await task;
            }
            catch (Messaging<Agent>.MessagingException ex)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(ex.Exception);

                Console.ForegroundColor = color;
            }
        }

        private static void Execute(Action action)
        {
            try
            {
                action();
            }
            catch (Messaging<Agent>.MessagingException ex)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(ex.Exception);

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

        private static async Task<Messaging<Agent>.IResponse> SendAndReciveAsync(Messaging<Agent>.IRequest request)
        {
            try
            {
                var result = await request.Reciver.Actor.Ask(request, TimeSpan.FromSeconds(2));
                return result as Messaging<Agent>.IResponse;
            }
            catch (Exception exception)
            {
                return request.Fault(exception);
            }
        }

        private static void SendMessage(Messaging<IActorRef>.IMessage message)
        {
            message.Reciver.Tell(message);
        }
        private static void SendMessage(Messaging<Agent>.IMessage message)
        {
            message.Reciver.Actor.Tell(message);
            
        }
    }

    public class Greet : Messaging<IActorRef>.Message<string>
    {
        public Greet(IActorRef sender, IActorRef reciver, string data) : base(sender, reciver, data)
        { }
        public Greet(IActorRef reciver, string data) : base(null, reciver, data)
        {
        }
    }


    public class Actor1 : ReceiveActor
    {
        public Actor1()
        {
            Receive<Greet>(greet => OnGreet(greet));
            Receive<AgentGreet>(greet => OnGreet(greet));
            Receive<Messaging<Agent>.Request<DateTime>>(dtRequest => OnDateTimeRequest(dtRequest));
            Receive<Messaging<Agent>.Response<DateTime>>(response => OnDateTimeResponse(response));
        }

        private void OnDateTimeResponse(Messaging<Agent>.Response<DateTime> response)
        {
            Console.WriteLine($"resp: {response.Data}");
        }

        private void OnDateTimeRequest(Messaging<Agent>.Request<DateTime> request)
        {
            if(request.NeedResponseInInRequest)
            {
                if (request.Handled)
                {
                    Console.WriteLine($"resp in req: {request.Data}");
                }
                else
                {
                    Sender.Tell(request.ResponseInRequest(DateTime.Now));
                }
            }
            else
                Sender.Tell(request.Response(DateTime.Now));
        }

        private void OnGreet(Greet greet)
        {
            Console.WriteLine(greet.Data);
        }

        private void OnGreet(AgentGreet greet)
        {
            Console.WriteLine(greet.Data);
        }
    }

    public class Agent2 : Agent
    {
        public Agent2(IActorRefFactory system) : base(system.ActorOf<Actor1>())
        {}

        public DateTime RequestDateTime()
        {
            return Messaging<Agent>.SendRequest<DateTime>(this);
        }
        public async Task<DateTime> RequestDateTimeAsync()
        {
            return await Messaging<Agent>.SendRequestAsync<DateTime>(this);
        }
    }

    public class AgentGreet : Messaging<Agent>.Message<string>
    {
        public AgentGreet(Agent reciver, string data = null) : base(null, reciver, data)
        { }
        public AgentGreet(Agent sender, Agent reciver, string data = null) : base(sender, reciver, data)
        {
        }
    }

    public abstract class Agent : BaseAgent
    {
        public IActorRef Actor;

        public Agent(IActorRef actor)
        {
            Actor = actor;
        }

        protected override ICompatibilitiesAgent CompatibilitiesAgent { get; }
        protected override void RegisterBehaviors()
        {
            
        }

        protected override bool IsEquals(IAgent other)
        {
            return ReferenceEquals(this, other);
        }
    }
}
