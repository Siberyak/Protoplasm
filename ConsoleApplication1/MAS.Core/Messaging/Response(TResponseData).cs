using System;

namespace MAS.Core
{
    public static partial class Messaging<TMessanger>
    {
        public class Response<TResponseData> : Response<TResponseData, TResponseData>
        {
            public Response(Request<TResponseData> request, TResponseData data = default(TResponseData)) : base(request, data)
            {
            }
        }
    }

    public static partial class Messaging<TMessanger>
    {
        public class FaultedRequest : Message, IFaultedReauest
        {
            public FaultedRequest(IRequest request, Exception exception)
            {
                Request = request;
                Exception = exception;
            }

            public override TMessanger Sender => Request.Reciver;
            public override TMessanger Reciver => Request.Sender;
            public IRequest Request { get; }
            public Exception Exception { get; }
        }
    }
}