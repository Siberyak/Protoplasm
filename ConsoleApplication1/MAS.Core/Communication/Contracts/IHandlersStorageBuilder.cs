using System;

namespace MAS.Core
{
    public interface IHandlersStorageBuilder<TRespondent>
    {
        IHandlersStorageBuilder<TRespondent> Asked<TQuestion, TAnswer>(Func<TRespondent, TQuestion, TAnswer> handler);
        IHandlersStorageBuilder<TRespondent> Asked<TQuestion, TAnswer>(Func<TRespondent, TQuestion, bool> predicate, Func<TRespondent, TQuestion, TAnswer> handler);

        IHandlersStorageBuilder<TRespondent> Requested<TData>(Func<TRespondent, TData> handler);

        IHandlersStorageBuilder<TRespondent> Told<TMessage>(Action<TRespondent, TMessage> handler);

        IHandlersStorageBuilder<TRespondent> Told<TMessage>(Func<TRespondent, TMessage, bool> predicate, Action<TRespondent, TMessage> handler);

        //public SourcedQuestionHandler<T, TQuestion> Question<TQuestion>(TQuestion question)
        //{
        //    return new SourcedQuestionHandler<T, TQuestion>(_storage, question);
        //}
    }
}