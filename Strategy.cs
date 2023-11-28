namespace DIContainers
{
    public interface IStrategy
    {
        public string Name { get; }

        public void Run();
    }

    public class StrategyA : IStrategy
    {
        public string Name { get; } = "StrategyA";

        public StrategyA()
        {
            Console.WriteLine("The strategy A has been constructed.");
        }

        public void Run()
        {
            Console.WriteLine($"{Name} is running.");
        }
    }

    public class StrategyB : IStrategy
    {
        public string Name { get; } = "StrategyB";

        public StrategyB() 
        {
            Console.WriteLine("The strategy B is infrequently used and expensive to construct");
            Thread.Sleep(2000);
            Console.WriteLine("The strategy B has been constructed.");
        }

        public void Run()
        {
            Console.WriteLine($"{Name} is running.");
        }
    }
}
