using System.Threading.Tasks;
using MAS.Core.Contracts;

namespace MAS.Core
{
    public class Request<T> : Messaging<IAgent>.Request<T>
    {
        public Request(IAgent sender, IAgent reciver, T data = default(T)) : base(sender, reciver, data)
        {
        }
    }

    public class Response<T1, T2> : Messaging<IAgent>.Response<T1, T2>
    {
        public Response(Messaging<IAgent>.Request<T1> request, T2 data = default(T2)) : base(request, data)
        {
        }
    }

    public class Response<T> : Response<T, T>
    {
        public Response(Messaging<IAgent>.Request<T> request, T data = default(T)) : base(request, data)
        {
        }
    }

    public static class AgentExtender
    {
 
    }
}