using System;

namespace MAS.Core
{
    public static partial class Messaging<TMessanger>
    {
        public class Request<TRequestData> : Message<TRequestData>, IRequest<TRequestData>, IResponse, IFaultedReauest
        {
            public bool NeedResponseInInRequest { get; set; }
            public bool Handled { get; set; }

            public Request(TMessanger sender, TMessanger reciver, TRequestData data = default(TRequestData), bool needResponseInInRequest = false)
                : base(sender, reciver, data)
            {
                NeedResponseInInRequest = needResponseInInRequest;
            }

            public Response<TRequestData> Response(TRequestData data)
            {
                return new Response<TRequestData>(this, data);
            }

            public Response<TRequestData, T2> Response<T2>(T2 data)
            {
                return new Response<TRequestData, T2>(this, data);
            }

            public Request<TRequestData> ResponseInRequest(TRequestData data)
            {
                Data = data;
                return this;
            }

            IRequest IResponse.Request => this;

            private IFaultedReauest _faulted;

            public bool IsFaulted => _faulted != null;

            Exception IFaultedReauest.Exception => _faulted.Exception;

            IFaultedReauest IRequest.Fault(Exception exception)
            {
                _faulted = new FaultedRequest(this, exception);

                return NeedResponseInInRequest ? this : _faulted;
            }
        }
    }
}