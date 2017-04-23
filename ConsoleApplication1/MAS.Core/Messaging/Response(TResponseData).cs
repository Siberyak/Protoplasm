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
}