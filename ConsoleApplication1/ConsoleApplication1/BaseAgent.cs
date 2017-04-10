namespace ConsoleApplication1
{
    public abstract class BaseAgent
    {
        public void Initialize()
        {
            RegisterBehaviors();
        }

        protected abstract void RegisterBehaviors();
    }
}