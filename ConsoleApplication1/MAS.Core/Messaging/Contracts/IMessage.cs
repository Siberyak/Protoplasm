namespace MAS.Core
{
    public static partial class Messaging<TMessanger>
    {
        public interface IMessage
        {
            TMessanger Sender { get; }
            TMessanger Reciver { get; }
        }
    }
}