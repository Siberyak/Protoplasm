using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MAS.Core.Compatibility.Contracts;

namespace MAS.Core
{
 
    public class HandlersStorage<TRespondent> : IHandlersStorageBuilder<TRespondent>
    {
        private readonly Dictionary<Type, List<HandlerInfo>> _handlers
    = new Dictionary<Type, List<HandlerInfo>>();

        private List<HandlerInfo> GetHandlersInfos<TKeyType>()
        {
            List<HandlerInfo> infos;
            if (!_handlers.TryGetValue(typeof(TKeyType), out infos))
                _handlers.Add(typeof(TKeyType), infos = new List<HandlerInfo>());
            return infos;
        }

        private bool FindHandlerInfos<T>(out List<HandlerInfo> infos)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out infos))
                return true;

            if (!type.IsInterface)
                return false;

            type = _handlers.Keys.FirstOrDefault(x => type.IsAssignableFrom(x));
            return type != null && _handlers.TryGetValue(type, out infos);
        }

        public bool Tell<TMessage>(TRespondent source, TMessage message)
        {
            List<HandlerInfo> infos;
            return FindHandlerInfos<TMessage>(out infos)
                   && infos.Any(x => x.TryProcessTold(source, message));
        }

        public bool Ask<TQuestion, TAnswer>(TRespondent source, TQuestion question, out TAnswer answer)
        {
            answer = default(TAnswer);

            List<HandlerInfo> infos;
            if (!FindHandlerInfos<TQuestion>(out infos))
                return false;

            foreach (var x in infos)
            {
                if (x.TryProcessAsked(source, question, out answer))
                    return true;
            }

            return false;
        }


        public bool Request<TData>(TRespondent source, out TData data)
        {
            data = default(TData);

            List<HandlerInfo> infos;
            if (!FindHandlerInfos<TData>(out infos))
                return false;

            foreach (var x in infos)
            {
                if (x.TryProcessRequested(source, out data))
                    return true;
            }

            return false;
        }

        public IHandlersStorageBuilder<TRespondent> Asked<TQuestion, TAnswer>(Func<TRespondent, TQuestion, TAnswer> handler)
        {
            return Asked(null, handler);
        }

        public IHandlersStorageBuilder<TRespondent> Asked<TQuestion, TAnswer>(Func<TRespondent, TQuestion, bool> predicate, Func<TRespondent, TQuestion, TAnswer> handler)
        {
            var infos = GetHandlersInfos<TQuestion>();
            infos.Add(new HandlerInputOutputInfo<TRespondent, TQuestion, TAnswer>(predicate, handler));
            return this;
        }


        public IHandlersStorageBuilder<TRespondent> Requested<TData>(Func<TRespondent, TData> handler)
        {
            var infos = GetHandlersInfos<TData>();
            infos.Add(new HandlerOutputInfo<TRespondent, TData>(handler));
            return this;
        }


        public IHandlersStorageBuilder<TRespondent> Told<TMessage>(Func<TRespondent, TMessage, bool> predicate, Action<TRespondent, TMessage> handler)
        {
            var infos = GetHandlersInfos<TMessage>();
            infos.Add(new HandlerInputInfo<TRespondent, TMessage>(predicate, handler));
            return this;
        }


        public IHandlersStorageBuilder<TRespondent> Told<TMessage>(Action<TRespondent, TMessage> handler)
        {
            return Told(null, handler);
        }

        public QuestionBuilder<TRespondent, TQuestion, TAnswer> Question<TQuestion, TAnswer>()
        {
            return new QuestionBuilder<TRespondent, TQuestion, TAnswer>(this);
        }
    }
}
