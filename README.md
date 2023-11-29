The objectives of this project are: 
1. To demonstrate how to use Microsoft.FeatureManagement with other dependency injection frameworks.
2. To explain the reason why people might prefer to use the open-source dependency injection framework, such as AutoFac, instead of `Microsoft.Extensions.DependencyInjection` (MEDI).

## How to use Microsoft.FeatureManagement with other third-party DI containers

There are some popular third-party dependency injection frameworks of .NET. ([ref1](https://learn.microsoft.com/en-us/dotnet/architecture/porting-existing-aspnet-apps/dependency-injection-differences), [ref2](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines#default-service-container-replacement))

The Nuget downloads of the top 5 most popular frameworks are as below:
* Autofac: 256.0M
* Unity: 71.9M
* Castle.Windsor: 54.9M
* SimpleInjector: 41.5M
* Ninject: 41.2M

The project demonstrates how to register Feature Management services for these frameworks. 
All demos are listed in the `Demo.cs`.

### Autofac
``` C#
IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var builder = new ContainerBuilder();

builder.RegisterInstance(config).SingleInstance();

builder.RegisterType<ConfigurationFeatureDefinitionProvider>().As<IFeatureDefinitionProvider>();

builder.RegisterInstance(new FeatureManagementOptions()).SingleInstance();

builder.RegisterInstance(LoggerFactory.Create(builder => builder.AddConsole()))
    .As<ILoggerFactory>().SingleInstance();

builder.Register(c => new FeatureManager(
    c.Resolve<IFeatureDefinitionProvider>(),
    c.Resolve<FeatureManagementOptions>())
{
    FeatureFilters = c.Resolve<IEnumerable<IFeatureFilterMetadata>>(),
    Logger = c.Resolve<ILoggerFactory>().CreateLogger<FeatureManager>()
}).As<IFeatureManager>()
    .SingleInstance();

builder.RegisterInstance(Options.Create(new TargetingEvaluationOptions()))
    .As<IOptions<TargetingEvaluationOptions>>()
    .SingleInstance();

var targetingContextAccessor = new OnDemandTargetingContextAccessor();

builder.RegisterInstance(targetingContextAccessor)
    .As<ITargetingContextAccessor>()
    .SingleInstance();

builder.RegisterType<TargetingFilter>()
    .As<IFeatureFilterMetadata>()
    .SingleInstance();

var container = builder.Build();

IFeatureManager featureManager = container.Resolve<IFeatureManager>();
```

### Unity
``` C#
IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var container = new UnityContainer();

container.RegisterInstance(config, InstanceLifetime.Singleton);

container.RegisterType<IFeatureDefinitionProvider, ConfigurationFeatureDefinitionProvider>(TypeLifetime.Singleton);

container.RegisterInstance(new FeatureManagementOptions(), InstanceLifetime.Singleton);

container.RegisterInstance(LoggerFactory.Create(builder => builder.AddConsole()), InstanceLifetime.Singleton);

container.RegisterFactory<IFeatureManager>(f => new FeatureManager(
    f.Resolve<IFeatureDefinitionProvider>(),
    f.Resolve<FeatureManagementOptions>())
{
    FeatureFilters = f.ResolveAll<IFeatureFilterMetadata>(),
    Logger = f.Resolve<ILoggerFactory>().CreateLogger<FeatureManager>(),
}, FactoryLifetime.Singleton);

container.RegisterInstance(Options.Create(new TargetingEvaluationOptions()), InstanceLifetime.Singleton);

var targetingContextAccessor = new OnDemandTargetingContextAccessor();

container.RegisterInstance<ITargetingContextAccessor>(targetingContextAccessor, InstanceLifetime.Singleton);

container.RegisterType<IFeatureFilterMetadata, TargetingFilter>("Targeting", TypeLifetime.Singleton);

IFeatureManager featureManager = container.Resolve<IFeatureManager>();
```

### Castle.Windsor
``` C#
IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var container = new WindsorContainer();

container.Register(Component.For<IConfiguration>()
    .Instance(config)
    .LifestyleSingleton());

container.Register(Component.For<IFeatureDefinitionProvider>()
    .ImplementedBy<ConfigurationFeatureDefinitionProvider>()
    .LifestyleSingleton());

container.Register(Component.For<FeatureManagementOptions>()
    .Instance(new FeatureManagementOptions())
    .LifestyleSingleton());

container.Register(Component.For<ILoggerFactory>()
    .Instance(LoggerFactory.Create(builder => builder.AddConsole()))
    .LifestyleSingleton());

container.Register(Component.For<IFeatureManager>().UsingFactoryMethod(kernel => new FeatureManager(
    kernel.Resolve<IFeatureDefinitionProvider>(),
    kernel.Resolve<FeatureManagementOptions>())
{
    FeatureFilters = kernel.ResolveAll<IFeatureFilterMetadata>(),
    Logger = kernel.Resolve<ILoggerFactory>().CreateLogger<FeatureManager>()
}));

container.Register(Component.For<IOptions<TargetingEvaluationOptions>>()
    .Instance(Options.Create(new TargetingEvaluationOptions()))
    .LifestyleSingleton());

var targetingContextAccessor = new OnDemandTargetingContextAccessor();

container.Register(Component.For<ITargetingContextAccessor>()
    .Instance(targetingContextAccessor)
    .LifestyleSingleton());

container.Register(Component.For<IFeatureFilterMetadata>()
    .ImplementedBy<TargetingFilter>()
    .LifestyleSingleton());

IFeatureManager featureManager = container.Resolve<IFeatureManager>();
```

### SimpleInjector
``` C#
IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var container = new Container();

container.RegisterSingleton(typeof(IConfiguration), () => config);

container.RegisterSingleton<IFeatureDefinitionProvider, ConfigurationFeatureDefinitionProvider>();

container.RegisterSingleton(typeof(FeatureManagementOptions), () => new FeatureManagementOptions());

container.RegisterSingleton(typeof(ILoggerFactory), () => LoggerFactory.Create(builder => builder.AddConsole()));

container.RegisterSingleton(typeof(IFeatureManager), () => new FeatureManager(
    container.GetInstance<IFeatureDefinitionProvider>(),
    container.GetInstance<FeatureManagementOptions>())
{
    FeatureFilters = container.GetAllInstances<IFeatureFilterMetadata>(),
    Logger = container.GetInstance<ILoggerFactory>().CreateLogger<FeatureManager>()
});

container.RegisterSingleton(typeof(IOptions<TargetingEvaluationOptions>), () => Options.Create(new TargetingEvaluationOptions()));

var targetingContextAccessor = new OnDemandTargetingContextAccessor();

container.RegisterSingleton(typeof(ITargetingContextAccessor), () => targetingContextAccessor);

container.Collection.Append<IFeatureFilterMetadata, TargetingFilter>(Lifestyle.Singleton);

IFeatureManager featureManager = container.GetInstance<IFeatureManager>();
```

### Ninject
``` C#
IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var kernel = new StandardKernel();

kernel.Bind<IConfiguration>()
    .ToConstant(config)
    .InSingletonScope();

kernel.Bind<IFeatureDefinitionProvider>()
    .To<ConfigurationFeatureDefinitionProvider>()
    .InSingletonScope();

kernel.Bind<FeatureManagementOptions>()
    .ToConstant(new FeatureManagementOptions())
    .InSingletonScope();

kernel.Bind<ILoggerFactory>()
    .ToConstant(LoggerFactory.Create(builder => builder.AddConsole()))
    .InSingletonScope();

kernel.Bind<IFeatureManager>().ToMethod(c => new FeatureManager(
    c.Kernel.Get<IFeatureDefinitionProvider>(),
    c.Kernel.Get<FeatureManagementOptions>())
{
    FeatureFilters = c.Kernel.Get<IEnumerable<IFeatureFilterMetadata>>(),
    Logger = c.Kernel.Get<ILoggerFactory>().CreateLogger<FeatureManager>()
}).InSingletonScope();

kernel.Bind<IOptions<TargetingEvaluationOptions>>()
    .ToConstant(Options.Create(new TargetingEvaluationOptions()))
    .InSingletonScope();

var targetingContextAccessor = new OnDemandTargetingContextAccessor();

kernel.Bind<ITargetingContextAccessor>()
    .ToConstant(targetingContextAccessor)
    .InSingletonScope();

kernel.Bind<IFeatureFilterMetadata>()
    .To<TargetingFilter>()
    .InSingletonScope();

IFeatureManager featureManager = kernel.Get<IFeatureManager>();
```

## The reason not to use `Microsoft.Extensions.DependencyInjection`
`Microsoft.Extensions.DependencyInjection` is the dependency injection framework for .NET provided by Microsoft, which designed to meet base-level functional needs. While MEDI offers a limited set of features, developers seeking enhanced capabilities have plenty of reasons to opt for alternative dependency injection frameworks that offer a more powerful feature set.

Taking AutoFac for comparison, I will provide two cases to illustrate the shortcomings of MEDI.

We have two apps which use MEDI and AutoFac respectively. The apps have some strategies which are injected by the DI container. The apps will run the specified strategy based on the input parameters.
``` C#
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
```
Besides, the apps have components A and B, which are classes inherited from the BaseComponent class.
``` C#
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
```

### 1. Register/Resolve by Key
Let's say we have multiple implementations of an interface and they need to be injected into different targets.
Before .NET 8.0, there is no way to register/resolve services by key.

Furthermore, there could be a scenario that we don't know which implementation to use until runtime.
In MEDI, we have to inject all implementations and resolve all of them to find the one to use.
Some implementations can be time-consuming to initialized and in-frequently to be used. AutoFac provides `IIndexed` which will only initialize the registered implementation while being get by the Key.

### 2. Properties Autowired
In MEDI, the public properties can only be injected through factory method.
``` C#
var services = new ServiceCollection();

services.AddSingleton<IMyLogger, MyLogger>();

services.AddSingleton(sp => new ComponentA()
{
    Logger = sp.GetRequiredService<IMyLogger>()
});

services.AddSingleton(sp => new ComponentB()
{
    Logger = sp.GetRequiredService<IMyLogger>()
});

services.AddSingleton<IApplication>(sp => new DemoAppWithMEDI(
    sp.GetRequiredService<IEnumerable<IStrategy>>())
{
    A = sp.GetRequiredService<ComponentA>(),
    B = sp.GetRequiredService<ComponentB>()
});
```
If the base class has properties which need to be injected. While registering its children classes, these inherited properties all need to be set manually.

In Autofac, public properties can be autowired:
``` C#
var builder = new ContainerBuilder();

builder.RegisterType<MyLogger>().As<IMyLogger>().SingleInstance();

builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(BaseComponent)))
    .Where(type => type.IsSubclassOf(typeof(BaseComponent)))
    .PropertiesAutowired()
    .AsSelf()
    .SingleInstance();

builder.RegisterType<DemoAppWithAutofac>()
    .PropertiesAutowired()
    .As<IApplication>()
    .SingleInstance();
```
