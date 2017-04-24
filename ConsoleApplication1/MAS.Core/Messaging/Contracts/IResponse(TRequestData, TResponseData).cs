using System;

namespace MAS.Core
{
    public static partial class Messaging<TMessanger>
    {
        public interface IResponse : IMessage
        {
             IRequest Request { get; }
        }

        public interface IFaultedReauest : IResponse
        {
            Exception Exception { get; }
        }

        public interface IResponse<TRequestData, TResponseData> : IResponse
        {
            new IRequest<TRequestData> Request { get; }
            TResponseData Data { get; }
        }
    }
}