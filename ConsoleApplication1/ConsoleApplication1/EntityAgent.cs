namespace ConsoleApplication1
{
    public abstract class EntityAgent<T> : BaseAgent
        where T : BaseEntity
    {
        protected EntityAgent(T entity)
        {
            Entity = entity;
        }

        protected T Entity { get; }
    }
}