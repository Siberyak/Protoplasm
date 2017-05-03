using System;

namespace MAS.Core
{
    public class QuestionBuilder<TRespondent, TBaseQuestion, TAnswer>
    {
        protected readonly HandlersStorage<TRespondent> _handlersStorage;

        public QuestionBuilder(HandlersStorage<TRespondent> handlersStorage)
        {
            _handlersStorage = handlersStorage;
        }

        public QuestionBuilder<TRespondent, TBaseQuestion, TAnswer> Asked(Func<TRespondent, TBaseQuestion, TAnswer> handler)
        {
            return Asked(null, handler);
        }

        public QuestionBuilder<TRespondent, TBaseQuestion, TAnswer> Asked(Func<TRespondent, TBaseQuestion, bool> predicate, Func<TRespondent, TBaseQuestion, TAnswer> handler)
        {
            _handlersStorage.Asked(predicate, handler);
            return this;
        }

        public QuestionBuilder<TRespondent, TBaseQuestion, TAnswer> Is<TQuestion>(Func<TRespondent, TQuestion, TAnswer> handler)
            where TQuestion : TBaseQuestion
        {
            return Is(null, handler);
        }

        public QuestionBuilder<TRespondent, TBaseQuestion, TAnswer> Is<TQuestion>(Func<TRespondent, TQuestion, bool> predicate, Func<TRespondent, TQuestion, TAnswer> handler)
            where TQuestion : TBaseQuestion
        {
            Func<TRespondent, TBaseQuestion, bool> basePredicate =
                (respondent, question) => question?.GetType() == typeof (TQuestion);

            Func<TRespondent, TBaseQuestion, TAnswer> baseHandle =
                (respondent, question) =>
                {
                    TAnswer answer;
                    return _handlersStorage.Ask(respondent, (TQuestion)question, out answer) ? answer : default(TAnswer);
                };

            _handlersStorage.Asked(basePredicate, baseHandle);
            _handlersStorage.Asked(predicate, handler);

            return this;
        }
    }
}