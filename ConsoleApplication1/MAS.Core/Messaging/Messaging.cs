using System;
using System.Threading;
using System.Threading.Tasks;
using MAS.Core.Contracts;

namespace MAS.Core
{
    public interface IMessanger
    { }

    public static class MessagingExtender
    {
        public static void Tell<TMessage>(this IMessanger reciver, TMessage message, IMessanger sender = null)
        {

        }

        public static Task<bool> Ask<TData>(this IMessanger reciver, out TData data, CancellationToken cancellationToken = default(CancellationToken), int? timeout = null, IMessanger sender = null)
        {
            data = default(TData);
            return Task.FromResult(false);
        }

        public static Task<bool> Ask<TQuestion, TAnswer>(this IMessanger reciver, TQuestion question, out TAnswer answer, CancellationToken cancellationToken = default(CancellationToken), int? timeout = null, IMessanger sender = null)
        {
            answer = default(TAnswer);
            return Task.FromResult(false);
        }
    }

    public static partial class Messaging<TMessanger>
    {
        public class MessagingException : Exception
        {
            public Exception Exception { get; }
            public IRequest FaultedRequest { get; }

            public MessagingException(IRequest request, Exception exception)
            {
                FaultedRequest = request;
                Exception = exception;
            }
        }

        private static TResponse ResultToResponse<TResponse>(IResponse result) 
            where TResponse : IResponse
        {
            CheckFault(result as IFaultedReauest);

            var response = result is TResponse ? (TResponse)result : default(TResponse);

            return response;
        }

        private static void CheckFault(IFaultedReauest faultedReauest)
        {
            if(faultedReauest?.Request.IsFaulted == true)
                throw new MessagingException(faultedReauest.Request, faultedReauest.Exception);
        }

        private static TResponseData DataInResponse<TRequestData, TResponseData>(IResponse<TRequestData, TResponseData> response)
        {
            return response != null
                ? response.Data
                : default(TResponseData);
        }

        private static TRequestData DataInRequest<TRequestData>(IRequest<TRequestData> request)
        {
            if (request == null)
                return default(TRequestData);

            //if(request.IsFaulted)
                return request.Data;
        }

        // === Sync ============================================


        public static Action<IMessage> SendDelegate;
            

        public static void SendMessage<TMessage>(TMessage message)
            where TMessage : IMessage
        {
            SendDelegate(message);
        }


        public static TRequestData SendRequest<TRequestData>(TMessanger reciver, TMessanger sender = default(TMessanger),TRequestData requestData = default(TRequestData), bool inRequest = true)
        {
            var request = new Request<TRequestData>(sender, reciver, requestData, inRequest);

            return inRequest
                ? DataInRequest(SendRequestReciveResponse<Request<TRequestData>, Request<TRequestData>>(request))
                : DataInResponse(SendRequestReciveResponse<Request<TRequestData>, Response<TRequestData>>(request));
        }

        public static TResponseData SendRequest<TRequestData, TResponseData>(TMessanger reciver, TMessanger sender = default(TMessanger), TRequestData requestData = default(TRequestData))
        {
            var request = new Request<TRequestData>(sender, reciver, requestData);
            return DataInResponse(SendRequestReciveResponse<Request<TRequestData>, Response<TRequestData, TResponseData>>(request));
        }

        public static Func<IRequest, IResponse> SendAndReciveDelegate;

        public static TResponse SendRequestReciveResponse<TRequest, TResponse>(TRequest request)
            where TRequest : IRequest
            where TResponse : IResponse
        {
            var result = SendAndReciveDelegate(request);
            return ResultToResponse<TResponse>(result);
        }


        // === Async ============================================

        public static Func<IMessage, Task> SendAsyncDelegate;

        public static async Task SendMessageAsync<TMessage>(TMessage message)
            where TMessage : IMessage
        {
            await SendAsyncDelegate(message);
        }

        public static async Task<TRequestData> SendRequestAsync<TRequestData>(TMessanger reciver, TMessanger sender = default(TMessanger), TRequestData requestData = default(TRequestData), bool inRequest = true)
        {
            var request = new Request<TRequestData>(sender, reciver, requestData, inRequest);
            if (inRequest)
            {
                var response = await SendRequestReciveResponseAsync<Request<TRequestData>, Request<TRequestData>>(request);
                return DataInRequest(response);
            }
            else
            {
                var response = await SendRequestReciveResponseAsync<Request<TRequestData>, Response<TRequestData>>(request);
                return DataInResponse(response);
            }
        }

        public static async Task<TResponseData> SendRequestAsync<TRequestData, TResponseData>(TMessanger reciver, TMessanger sender = default(TMessanger), TRequestData requestData = default(TRequestData))
        {
            var request = new Request<TRequestData>(sender, reciver, requestData);
            var response = await SendRequestReciveResponseAsync<Request<TRequestData>, Response<TRequestData, TResponseData>>(request);
            return DataInResponse(response);
        }

        public static Func<IRequest, Task<IResponse>> SendAndReciveAsyncDelegate;

        public static async Task<TResponse> SendRequestReciveResponseAsync<TRequest, TResponse>(TRequest request)
            where TRequest : IRequest
            where TResponse : IResponse
        {
            var result = await SendAndReciveAsyncDelegate(request);
            return ResultToResponse<TResponse>(result);
        }
    }
}