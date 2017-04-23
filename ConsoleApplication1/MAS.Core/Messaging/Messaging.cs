using System;
using System.Threading.Tasks;

namespace MAS.Core
{
    public static partial class Messaging<TMessanger>
    {
        private static TResponseData DataInResponse<TRequestData, TResponseData>(IResponse<TRequestData, TResponseData> response)
        {
            return response != null
                ? response.Data
                : default(TResponseData);
        }

        private static TRequestData DataInRequest<TRequestData>(IRequest<TRequestData> request)
        {
            return request != null
                ? request.Data
                : default(TRequestData);
        }

        // === Sync ============================================


        public static Action<IMessage> SendMessageDelegate;
            

        public static void SendMessage<TMessage>(TMessage message)
            where TMessage : IMessage
        {
            SendMessageDelegate(message);
        }


        public static TRequestData SendRequest<TRequestData>(TMessanger sender, TMessanger reciver, TRequestData requestData = default(TRequestData), bool inRequest = true)
        {
            var request = new Request<TRequestData>(sender, reciver, requestData);

            return inRequest
                ? DataInRequest(SendAndWaitResponse<Request<TRequestData>, Request<TRequestData>>(request))
                : DataInResponse(SendAndWaitResponse<Request<TRequestData>, Response<TRequestData>>(request));
        }

        public static TResponseData SendRequest<TRequestData, TResponseData>(TMessanger sender, TMessanger reciver, TRequestData requestData = default(TRequestData))
        {
            var request = new Request<TRequestData>(sender, reciver, requestData);
            return DataInResponse(SendAndWaitResponse<Request<TRequestData>, Response<TRequestData, TResponseData>>(request));
        }

        public static TResponse SendAndWaitResponse<TRequest, TResponse>(TRequest request)
        {
            return default(TResponse);
        }

        // === Async ============================================

        public static async Task SendMessageAsync<TMessage>(TMessage message)
            where TMessage : IMessage
        {
            throw new NotImplementedException();
        }

        public static async Task<TRequestData> SendRequestAsync<TRequestData>(TMessanger sender, TMessanger reciver, TRequestData requestData = default(TRequestData), bool inRequest = true)
        {
            var request = new Request<TRequestData>(sender, reciver, requestData);
            if (inRequest)
            {
                var response = await SendAndWaitResponseAsync<Request<TRequestData>, Request<TRequestData>>(request);
                return DataInRequest(response);
            }
            else
            {
                var response = await SendAndWaitResponseAsync<Request<TRequestData>, Response<TRequestData>>(request);
                return DataInResponse(response);
            }
        }

        public static async Task<TResponseData> SendRequestAsync<TRequestData, TResponseData>(TMessanger sender, TMessanger reciver, TRequestData requestData = default(TRequestData))
        {
            var request = new Request<TRequestData>(sender, reciver, requestData);
            var response = await SendAndWaitResponseAsync<Request<TRequestData>, Response<TRequestData, TResponseData>>(request);
            return DataInResponse(response);
        }

        public static Task<TResponse> SendAndWaitResponseAsync<TRequest, TResponse>(TRequest request)
        {
            throw new NotImplementedException();

            return Task.FromResult(default(TResponse));
        }
    }
}