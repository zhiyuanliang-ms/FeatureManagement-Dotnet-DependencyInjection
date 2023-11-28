using Autofac.Features.Indexed;

namespace DIContainers
{
    public interface IApplication
    {
        public ComponentA A { get; }

        public ComponentB B { get; }

        public void RunStrategy(string strategyName);
    }

    public class DemoAppWithMEDI : IApplication
    {
        private readonly IEnumerable<IStrategy> _strategies;

        public ComponentA A { get; init; }

        public ComponentB B { get; init; }

        public DemoAppWithMEDI(IEnumerable<IStrategy> strategies)
        {
            _strategies = strategies;
        }

        public void RunStrategy(string strategyName)
        {
            _strategies.FirstOrDefault(s => s.Name == strategyName)?.Run();
        }
    }

    public class DemoAppWithAutofac : IApplication
    {
        private readonly IIndex<string, IStrategy> _strategies;

        public ComponentA A { get; init; }

        public ComponentB B { get; init; }

        public DemoAppWithAutofac(IIndex<string, IStrategy> strategies)
        {
            _strategies = strategies;
        }

        public void RunStrategy(string strategyName)
        {
            if (_strategies.TryGetValue(strategyName, out IStrategy strategy))
            {
                strategy.Run();
            }
        }
    }
}
