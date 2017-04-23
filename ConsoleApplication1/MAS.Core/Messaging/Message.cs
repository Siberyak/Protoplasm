namespace MAS.Core
{
    public static partial class Messaging<TMessanger>
    {
        public abstract class Message : IMessage
        {
            public abstract TMessanger Sender { get; }
            public abstract TMessanger Reciver { get; }

        }
    }
}