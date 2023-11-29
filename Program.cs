namespace DIContainers
{
    class Program
    {
        static void Main()
        {
            Demo.ResolveMultipleImplementationsMEDI();

            Demo.InjectPropertyMEDI();

            Demo.ResolveMultipleImplementationsAutofac();

            Demo.InjectPropertyAutofac();

            //Demo.UseFeatureManagementWithAutofac();

            //Demo.UseFeatureManagementWithUnity();

            //Demo.UseFeatureManagementWithCastleWindsor();

            //Demo.UseFeatureManagementWithSimpleInjector();

            //Demo.UseFeatureManagementWithNinject();
        }
    }
}