using Autofac;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Ninject;
using System.Reflection;
using Unity;

namespace DIContainers
{
    public static class Demo
    {
        public static void DemoMEDI()
        {
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

            services.AddTransient<IStrategy, StrategyA>();

            services.AddTransient<IStrategy, StrategyB>();

            services.AddSingleton<IApplication>(sp => new DemoAppWithMEDI(
                sp.GetRequiredService<IEnumerable<IStrategy>>())
            {
                A = sp.GetRequiredService<ComponentA>(),
                B = sp.GetRequiredService<ComponentB>()
            });

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IApplication app = serviceProvider.GetRequiredService<IApplication>();

            app.RunStrategy("StrategyA");

            app.RunStrategy("StrategyA");

            app.A.DoSomething();

            app.A.Logger.Info("BaseComponent A of the App with MEDI is doing something.");

            app.B.DoSomething();

            app.B.Logger.Info("BaseComponent B of the App with MEDI is doing something.");
        }

        public static void DemoAutofac()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<MyLogger>().As<IMyLogger>().SingleInstance();

            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(BaseComponent)))
                .Where(type => type.IsSubclassOf(typeof(BaseComponent)))
                .PropertiesAutowired()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<StrategyA>().Keyed<IStrategy>("StrategyA");

            builder.RegisterType<StrategyB>().Keyed<IStrategy>("StrategyB");

            builder.RegisterType<DemoAppWithAutofac>()
                .PropertiesAutowired()
                .As<IApplication>()
                .SingleInstance();

            var container = builder.Build();

            var app = container.Resolve<IApplication>();

            app.RunStrategy("StrategyA");

            app.RunStrategy("StrategyA");

            app.A.DoSomething();

            app.A.Logger.Info("BaseComponent A of the App with Autofac is doing something.");

            app.B.DoSomething();

            app.B.Logger.Info("BaseComponent B of the App with Autofac is doing something.");
        }

        public static async void UseFeatureManagementWithAutofac()
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(config).As<IConfiguration>().SingleInstance();

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
            }).As<IFeatureManager>().SingleInstance();

            builder.RegisterInstance(Options.Create(new TargetingEvaluationOptions()))
                .As<IOptions<TargetingEvaluationOptions>>().SingleInstance();

            var targetingContextAccessor = new OnDemandTargetingContextAccessor();

            builder.RegisterInstance(targetingContextAccessor).As<ITargetingContextAccessor>().SingleInstance();

            builder.RegisterType<TargetingFilter>().As<IFeatureFilterMetadata>().SingleInstance();

            var container = builder.Build();

            IFeatureManager featureManager = container.Resolve<IFeatureManager>();

            var users = new List<string>()
            {
                "Jeff",
                "Sam"
            };

            const string feature = "Beta";

            foreach (var user in users)
            {
                targetingContextAccessor.Current = new TargetingContext
                {
                    UserId = user
                };

                Console.WriteLine($"{feature} is {(await featureManager.IsEnabledAsync(feature) ? "enabled" : "disabled")} for {user}.");
            }
        }

        public static async void UseFeatureManagementWithUnity()
        {
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

            var users = new List<string>()
            {
                "Jeff",
                "Sam"
            };

            const string feature = "Beta";

            foreach (var user in users)
            {
                targetingContextAccessor.Current = new TargetingContext
                {
                    UserId = user
                };

                Console.WriteLine($"{feature} is {(await featureManager.IsEnabledAsync(feature) ? "enabled" : "disabled")} for {user}.");
            }
        }

        public static async void UseFeatureManagementWithNinject()
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var kernel = new StandardKernel();

            kernel.Bind<IConfiguration>().ToConstant(config).InSingletonScope();

            kernel.Bind<IFeatureDefinitionProvider>().To<ConfigurationFeatureDefinitionProvider>().InSingletonScope();

            kernel.Bind<FeatureManagementOptions>().ToConstant(new FeatureManagementOptions()).InSingletonScope();

            kernel.Bind<ILoggerFactory>().ToConstant(LoggerFactory.Create(builder => builder.AddConsole())).InSingletonScope();

            kernel.Bind<IFeatureManager>().ToMethod(c => new FeatureManager(
                c.Kernel.Get<IFeatureDefinitionProvider>(),
                c.Kernel.Get<FeatureManagementOptions>())
            {
                FeatureFilters = c.Kernel.Get<IEnumerable<IFeatureFilterMetadata>>(),
                Logger = c.Kernel.Get<ILoggerFactory>().CreateLogger<FeatureManager>()
            }).InSingletonScope();
            
            kernel.Bind<IOptions<TargetingEvaluationOptions>>().ToConstant(Options.Create(new TargetingEvaluationOptions())).InSingletonScope();

            var targetingContextAccessor = new OnDemandTargetingContextAccessor();

            kernel.Bind<ITargetingContextAccessor>().ToConstant(targetingContextAccessor).InSingletonScope();

            kernel.Bind<IFeatureFilterMetadata>().To<TargetingFilter>().InSingletonScope();

            IFeatureManager featureManager = kernel.Get<IFeatureManager>();

            var users = new List<string>()
            {
                "Jeff",
                "Sam"
            };

            const string feature = "Beta";

            foreach (var user in users)
            {
                targetingContextAccessor.Current = new TargetingContext
                {
                    UserId = user
                };

                Console.WriteLine($"{feature} is {(await featureManager.IsEnabledAsync(feature) ? "enabled" : "disabled")} for {user}.");
            }
        }

        public static async void UseFeatureManagementWithCastleWindsor()
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var container = new WindsorContainer();

            container.Register(Component.For<IConfiguration>().Instance(config).LifestyleSingleton());

            container.Register(Component.For<IFeatureDefinitionProvider>().ImplementedBy<ConfigurationFeatureDefinitionProvider>().LifestyleSingleton());

            container.Register(Component.For<FeatureManagementOptions>().Instance(new FeatureManagementOptions()).LifestyleSingleton());

            container.Register(Component.For<ILoggerFactory>().Instance(LoggerFactory.Create(builder => builder.AddConsole())).LifestyleSingleton());

            container.Register(Component.For<IFeatureManager>().UsingFactoryMethod(kernel => new FeatureManager(
                kernel.Resolve<IFeatureDefinitionProvider>(),
                kernel.Resolve<FeatureManagementOptions>())
            {
                FeatureFilters = kernel.ResolveAll<IFeatureFilterMetadata>(),
                Logger = kernel.Resolve<ILoggerFactory>().CreateLogger<FeatureManager>()
            }));

            container.Register(Component.For<IOptions<TargetingEvaluationOptions>>().Instance(Options.Create(new TargetingEvaluationOptions())).LifestyleSingleton());

            var targetingContextAccessor = new OnDemandTargetingContextAccessor();

            container.Register(Component.For<ITargetingContextAccessor>().Instance(targetingContextAccessor).LifestyleSingleton());

            container.Register(Component.For<IFeatureFilterMetadata>().ImplementedBy<TargetingFilter>().LifestyleSingleton());

            IFeatureManager featureManager = container.Resolve<IFeatureManager>();

            var users = new List<string>()
            {
                "Jeff",
                "Sam"
            };

            const string feature = "Beta";

            foreach (var user in users)
            {
                targetingContextAccessor.Current = new TargetingContext
                {
                    UserId = user
                };

                Console.WriteLine($"{feature} is {(await featureManager.IsEnabledAsync(feature) ? "enabled" : "disabled")} for {user}.");
            }
        }
    }
}
