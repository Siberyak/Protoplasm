namespace MAS.Core
{
    public static partial class Messaging<TMessanger>
    {
        public class Message<TMessageData> : Message
        {
            public override TMessanger Sender { get; }
            public override TMessanger Reciver { get; }
            public TMessageData Data { get; protected set; }

            protected Message(TMessageData data = default(TMessageData))
            {
                Data = data;
            }

            public Message(TMessanger sender, TMessanger reciver, TMessageData data = default(TMessageData))
                : this(data)
            {
                Sender = sender;
                Reciver = reciver;
            }
        }
    }
}