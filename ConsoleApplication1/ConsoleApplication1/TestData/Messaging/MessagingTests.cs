using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using MAS.Core;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using Protoplasm.Utils;

namespace ConsoleApplication1.TestData.Messaging
{
    public class MessagingTests
    {
        public static void Test1()
        {
            var config = ConfigurationFactory.ParseString(@"
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


            Messaging<IActorRef>.SendMessageDelegate = SendMessage;


            ActorSystem system = ActorSystem.Create("MessagingTests-Test1", config);
            var actor1 = system.ActorOf<Actor1>("greeter");


            var message = new Greet(actor1, "Message 1");
            Messaging<IActorRef>.SendMessage(message);

            actor1.Tell(new Greet(actor1, "Message 2"));

            var actor2 = new Actor2(system);
            Messaging<Agent>.SendMessageDelegate = SendMessage;

            Messaging<Agent>.SendMessage(new AgentGreet(actor2, "AgetnGreetMessage"));

            Console.WriteLine("-= waiting =-");
            Console.ReadLine();

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

    public class Actor2 : Agent
    {
        public Actor2(ActorSystem system) : base(system.ActorOf<Actor1>())
        {}
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
