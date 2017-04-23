namespace MAS.Core
{
    public static partial class Messaging<TMessanger>
    {
        public class Request<TRequestData> : Message<TRequestData>, IRequest<TRequestData>
        {
            public Request(TMessanger sender, TMessanger reciver, TRequestData data = default(TRequestData))
                : base(sender, reciver, data)
            {
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
        }
    }
}