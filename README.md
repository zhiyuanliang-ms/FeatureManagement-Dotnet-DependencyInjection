The objectives of this project are: 
1. To demonstrate how to use Microsoft.FeatureManagement with other dependency injection frameworks.
2. To explain the reason why people might prefer to use the open-source dependency injection framework, such as AutoFac, instead of `Microsoft.Extensions.DependencyInjection` (MEDI).

There are some popular third-party dependency injection frameworks of .NET. ([ref1](https://learn.microsoft.com/en-us/dotnet/architecture/porting-existing-aspnet-apps/dependency-injection-differences), [ref2](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines#default-service-container-replacement))

The Nuget downloads of the top 5 most popular frameworks are as below:
* Autofac: 256.0M
* Unity: 71.9M
* Castle.Windsor: 54.9M
* SimpleInjector: 41.5M
* Ninject: 41.2M

The project demonstrates how to register Feature Management services for these frameworks. 
All demos are listed in the `Demo.cs`.

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
