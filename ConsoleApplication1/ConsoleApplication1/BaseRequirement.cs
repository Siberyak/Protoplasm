namespace ConsoleApplication1
{
    public abstract class BaseRequirement
    {
        protected abstract ConformResult Conformable(BaseAbility ability);
    }
}