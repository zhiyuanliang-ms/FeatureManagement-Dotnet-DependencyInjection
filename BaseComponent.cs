namespace DIContainers
{
    public interface IComponent
    {
    }

    public abstract class BaseComponent : IComponent
    {
        public IMyLogger Logger { get; set; }

        public abstract void DoSomething();
    }

    public class ComponentA : BaseComponent 
    { 
        public override void DoSomething()
        {
            Console.WriteLine("ComponentA is working.");
        }
    }

    public class ComponentB : BaseComponent
    {
        public override void DoSomething()
        {
            Console.WriteLine("ComponentB is working.");
        }
    }
}
