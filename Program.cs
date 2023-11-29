namespace DIContainers
{
    class Program
    {
        static void Main()
        {
            Demo.ResolveMultipleImplementationsWithMEDI();

            Demo.InjectPropertyWithMEDI();

            Demo.ResolveMultipleImplementationsWithAutofac();

            Demo.InjectPropertyWithAutofac();

            Demo.UseFeatureManagementWithAutofac();

            Demo.UseFeatureManagementWithUnity();

            Demo.UseFeatureManagementWithCastleWindsor();

            Demo.UseFeatureManagementWithSimpleInjector();

            Demo.UseFeatureManagementWithNinject();
        }
    }
}