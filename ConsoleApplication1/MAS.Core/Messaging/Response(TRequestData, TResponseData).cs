namespace MAS.Core
{
    public static partial class Messaging<TMessanger>
    {
        public class Response<TRequestData, TResponseData> : Message<TResponseData>, IResponse<TRequestData, TResponseData>
        {
            public override TMessanger Sender => Request.Reciver;
            public override TMessanger Reciver => Request.Sender;

            public Response(Request<TRequestData> request, TResponseData data = default(TResponseData)) : base(data)
            {
                Request = request;
            }

            public IRequest<TRequestData> Request { get; }
            IRequest IResponse.Request => Request;
        }
    }
}