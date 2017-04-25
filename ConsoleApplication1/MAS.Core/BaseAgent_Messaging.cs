using System;
using System.Collections.Generic;
using System.Linq;

namespace MAS.Core
{
    public abstract partial class BaseAgent
    {

        protected virtual HandlersStorage GetStaticHandlers()
        {
            return null;
        }

        protected virtual void InitPersonalHandlers(HandlersStorage handlers)
        {}

        private readonly HandlersStorage _personalHandlers = new HandlersStorage();

        public bool Tell<T, TMessage>(TMessage message)
        {
            return GetStaticHandlers()?.Tell((T)(object)this, message) == true ||  _personalHandlers.Tell((T)(object)this, message);
        }

        public bool Ask<T, TQuestion, TAnswer>(TQuestion question, out TAnswer answer)
        {
            answer = default(TAnswer);
            return GetStaticHandlers()?.Ask((T)(object)this, question, out answer) == true || _personalHandlers.Ask((T)(object)this, question, out answer);
        }

        public bool Request<T, TData>(out TData data)
        {
            data = default(TData);
            return GetStaticHandlers()?.Request((T)(object)this, out data) == true || _personalHandlers.Request((T)(object)this, out data);
        }
    }

    internal abstract class ProcessingInfo
    {
        public bool TryProcessRequested<T, TOut>(T source, out TOut output)
        {
            output = default(TOut);

            if (!Allowed<T, TOut>(source))
                return false;

            output = Process<T, TOut>(source);
            return true;
        }

        public bool TryProcessTold<T, TIn>(T source, TIn input)
        {
            if (!Allowed(source, input))
                return false;

            Process(source, input);
            return true;
        }

        public bool TryProcessAsked<T, TIn, TOut>(T source, TIn input, out TOut output)
        {
            output = default(TOut);

            if (!Allowed<T, TIn, TOut>(source, input))
                return false;

            output = Process<T, TIn, TOut>(source, input);
            return true;
        }

        protected virtual bool Allowed<T, TOut>(T source){throw new NotImplementedException();}

        protected virtual bool Allowed<T, TIn>(T source, TIn input){throw new NotImplementedException();}

        protected virtual bool Allowed<T, TIn, TOut>(T source, TIn input){throw new NotImplementedException();}

        protected virtual TOut Process<T, TOut>(T source) {throw new NotImplementedException();}

        protected virtual void Process<T, TIn>(T source, TIn input){throw new NotImplementedException();}

        protected virtual TOut Process<T, TIn, TOut>(T source, TIn input){throw new NotImplementedException();}
    }

    class ProcessingInputInfo<TSource, TInput> : ProcessingInfo
    {
        private readonly Action<TSource, TInput> _execute;
        protected readonly Func<TSource, TInput, bool> Predicate;


        public ProcessingInputInfo(Func<TSource, TInput, bool> predicate, Action<TSource, TInput> execute)
        {
            Predicate = predicate;
            _execute = execute;
        }


        protected override bool Allowed<T, TIn>(T source, TIn input)
        {
            return typeof(TSource).IsAssignableFrom(typeof(T)) 
                &&  typeof(TInput).IsAssignableFrom(typeof(TIn)) 
                && Predicate?.Invoke((TSource)(object)source, (TInput)(object)input) != false;
        }

        protected override void Process<T, TIn>(T source, TIn input)
        {
            _execute((TSource)(object)source, (TInput)(object)input);
        }
    }

    class ProcessingOutputInfo<TSource, TOutput> : ProcessingInfo
    {
        private readonly Func<TSource, TOutput> _evaluate;

        public ProcessingOutputInfo(Func<TSource, TOutput> evaluate)
        {
            _evaluate = evaluate;
        }

        protected override bool Allowed<T, TOut>(T source)
        {
            return typeof(TSource).IsAssignableFrom(typeof(T)) 
                && typeof(TOutput).IsAssignableFrom(typeof(TOut));
        }

        protected override TOut Process<T, TOut>(T source)
        {
            return (TOut)(object)_evaluate((TSource)(object)source);
        }
    }

    class ProcessingInputOutputInfo<TSource, TInput, TOutput> : ProcessingInputInfo<TSource, TInput>
    {
        private readonly Func<TSource, TInput, TOutput> _evaluate;

        public ProcessingInputOutputInfo(Func<TSource, TInput, bool> predicate, Func<TSource, TInput, TOutput> evaluate)
            : base(predicate, (source, input) => { throw new NotSupportedException(); })
        {
            _evaluate = evaluate;
        }

        protected override bool Allowed<T, TIn, TOut>(T source, TIn input)
        {
            return base.Allowed(source, input) && typeof(TOutput).IsAssignableFrom(typeof(TOut));
        }

        protected override TOut Process<T, TIn, TOut>(T source, TIn input)
        {
            TOutput output = _evaluate((TSource)(object)source, (TInput)(object)input);
            return (TOut)(object)output;
        }
    }

    public class HandlersStorage
    {
        public class Builder<T>
        {
            private readonly HandlersStorage _storage;

            internal Builder(HandlersStorage storage)
            {
                _storage = storage;
            }

            public Builder<T> Asked<TQuestion, TAnswer>(Func<T, TQuestion, TAnswer> handler)
            {
                return Asked(null, handler);
            }
            public Builder<T> Asked<TQuestion, TAnswer>(Func<T, TQuestion, bool> predicate, Func<T, TQuestion, TAnswer> handler)
            {
                _storage.Asked(predicate, handler);
                return this;
            }

            public Builder<T> Requested<TData>(Func<T, TData> handler)
            {
                _storage.Requested(handler);
                return this;
            }

            public Builder<T> Told<TMessage>(Action<T, TMessage> handler)
            {
                return Told(null, handler);
            }

            public Builder<T> Told<TMessage>(Func<T, TMessage, bool> predicate, Action<T, TMessage> handler)
            {
                _storage.Told(predicate, handler);
                return this;
            }
        }

        public Builder<T> GetBuilder<T>()
        {
            return new Builder<T>(this);
        } 

        private readonly Dictionary<Type, List<ProcessingInfo>> _messageHandlers
            = new Dictionary<Type, List<ProcessingInfo>>();

        private List<ProcessingInfo> GetProcessingInfos<T>()
        {
            List<ProcessingInfo> infos;
            if (!_messageHandlers.TryGetValue(typeof(T), out infos))
                _messageHandlers.Add(typeof(T), infos = new List<ProcessingInfo>());
            return infos;
        }

        public bool Tell<T, TMessage>(T source, TMessage message)
        {
            List<ProcessingInfo> infos;
            return _messageHandlers.TryGetValue(typeof(TMessage), out infos)
                && infos.Any(x => x.TryProcessTold(source, message));
        }

        public bool Ask<T, TQuestion, TAnswer>(T source, TQuestion question, out TAnswer answer)
        {
            answer = default(TAnswer);

            List<ProcessingInfo> infos;
            if (!_messageHandlers.TryGetValue(typeof(TQuestion), out infos))
                return false;

            foreach (var x in infos)
            {
                if (x.TryProcessAsked(source, question, out answer))
                    return true;
            }

            return false;
        }

        public bool Request<T,TData>(T source, out TData data)
        {
            data = default(TData);

            List<ProcessingInfo> infos;
            if (!_messageHandlers.TryGetValue(typeof(TData), out infos))
                return false;

            foreach (var x in infos)
            {
                if (x.TryProcessRequested(source, out data))
                    return true;
            }

            return false;
        }


        public void Asked<T, TQuestion, TAnswer>(Func<T, TQuestion, bool> predicate, Func<T, TQuestion, TAnswer> handler)
        {
            var infos = GetProcessingInfos<TQuestion>();
            infos.Add(new ProcessingInputOutputInfo<T, TQuestion, TAnswer>(predicate, handler));
        }


        public void Requested<T, TData>(Func<T, TData> handler)
        {
            var infos = GetProcessingInfos<TData>();
            infos.Add(new ProcessingOutputInfo<T, TData>(handler));
        }


        public void Told<T, TMessage>(Func<T, TMessage, bool> predicate, Action<T, TMessage> handler)
        {
            var infos = GetProcessingInfos<TMessage>();
            infos.Add(new ProcessingInputInfo<T, TMessage>(predicate, handler));
        }
    }
}