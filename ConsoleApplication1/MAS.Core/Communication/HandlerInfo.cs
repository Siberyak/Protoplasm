using System;

namespace MAS.Core
{
    internal abstract class HandlerInfo
    {
        public bool TryProcessRequested<TResp, TOut>(TResp respondent, out TOut output)
        {
            output = default(TOut);

            if (!Allowed<TResp, TOut>(respondent))
                return false;

            output = Process<TResp, TOut>(respondent);
            return true;
        }

        public bool TryProcessTold<TResp, TIn>(TResp respondent, TIn input)
        {
            if (!Allowed(respondent, input))
                return false;

            Process(respondent, input);
            return true;
        }

        public bool TryProcessAsked<TResp, TIn, TOut>(TResp respondent, TIn input, out TOut output)
        {
            output = default(TOut);

            if (!Allowed<TResp, TIn, TOut>(respondent, input))
                return false;

            output = Process<TResp, TIn, TOut>(respondent, input);
            return true;
        }

        protected virtual bool Allowed<TResp, TOut>(TResp respondent) { return false; }

        protected virtual bool Allowed<TResp, TIn>(TResp respondent, TIn input) { return false; }

        protected virtual bool Allowed<TResp, TIn, TOut>(TResp respondent, TIn input) { return false; }

        protected virtual TOut Process<TResp, TOut>(TResp respondent) { throw new NotImplementedException(); }

        protected virtual void Process<TResp, TIn>(TResp respondent, TIn input) { throw new NotImplementedException(); }

        protected virtual TOut Process<TResp, TIn, TOut>(TResp respondent, TIn input) { throw new NotImplementedException(); }
    }

    class HandlerInputInfo<TRespondent, TInput> : HandlerInfo
    {
        private readonly Action<TRespondent, TInput> _execute;
        protected readonly Func<TRespondent, TInput, bool> Predicate;


        public HandlerInputInfo(Func<TRespondent, TInput, bool> predicate, Action<TRespondent, TInput> execute)
        {
            Predicate = predicate;
            _execute = execute;
        }


        protected override bool Allowed<TResp, TIn>(TResp respondent, TIn input)
        {
            return typeof(TRespondent).IsAssignableFrom(typeof(TResp))
                && typeof(TInput).IsAssignableFrom(typeof(TIn))
                && Predicate?.Invoke((TRespondent)(object)respondent, (TInput)(object)input) != false;
        }

        protected override void Process<TResp, TIn>(TResp respondent, TIn input)
        {
            _execute((TRespondent)(object)respondent, (TInput)(object)input);
        }
    }

    class HandlerOutputInfo<TRespondent, TOutput> : HandlerInfo
    {
        private readonly Func<TRespondent, TOutput> _evaluate;

        public HandlerOutputInfo(Func<TRespondent, TOutput> evaluate)
        {
            _evaluate = evaluate;
        }

        protected override bool Allowed<TResp, TOut>(TResp respondent)
        {
            return typeof(TRespondent).IsAssignableFrom(typeof(TResp))
                && typeof(TOutput).IsAssignableFrom(typeof(TOut));
        }

        protected override TOut Process<TResp, TOut>(TResp respondent)
        {
            return (TOut)(object)_evaluate((TRespondent)(object)respondent);
        }
    }

    class HandlerInputOutputInfo<TRespondent, TInput, TOutput> : HandlerInputInfo<TRespondent, TInput>
    {
        private readonly Func<TRespondent, TInput, TOutput> _evaluate;

        public HandlerInputOutputInfo(Func<TRespondent, TInput, bool> predicate, Func<TRespondent, TInput, TOutput> evaluate)
            : base(predicate, (source, input) => { throw new NotSupportedException(); })
        {
            _evaluate = evaluate;
        }

        protected override bool Allowed<TResp, TIn, TOut>(TResp respondent, TIn input)
        {
            return base.Allowed(respondent, input) && typeof(TOutput).IsAssignableFrom(typeof(TOut));
        }

        protected override TOut Process<TResp, TIn, TOut>(TResp respondent, TIn input)
        {
            TOutput output = _evaluate((TRespondent)(object)respondent, (TInput)(object)input);
            return (TOut)(object)output;
        }
    }
}