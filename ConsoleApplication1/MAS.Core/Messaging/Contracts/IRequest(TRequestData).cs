using System;

namespace MAS.Core
{
    public static partial class Messaging<TMessanger>
    {
        public interface IRequest : IMessage
        {
            bool IsFaulted { get; }
            IFaultedReauest Fault(Exception exception);
        }

        public interface IRequest<TRequestData> : IRequest
        {
            TRequestData Data { get; }
        }
    }
}