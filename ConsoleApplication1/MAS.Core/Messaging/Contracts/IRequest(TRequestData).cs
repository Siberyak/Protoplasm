namespace MAS.Core
{
    public static partial class Messaging<TMessanger>
    {
        public interface IRequest<TRequestData> : IMessage
        {
            TRequestData Data { get; }
        }
    }
}