namespace MAS.Core
{
    public interface IRespondent
    {
        bool Ask<TQuestion, TAnswer>(TQuestion question, out TAnswer answer);
        bool Tell<TMessage>(TMessage message);
        bool Request<TData>(out TData data);
    }
}