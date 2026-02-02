using System.Runtime.CompilerServices;
using System.Runtime.Loader;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Setup()
    {
        var loadContext = AssemblyLoadContext.GetLoadContext(typeof(ModuleInitializer).Assembly)!;
        Console.WriteLine($"ModuleInitializer called in AssemblyLoadContext: {loadContext.Name}");

        const string key = "ModuleInitializer_Initialized";
        var previousLoadContext = AppDomain.CurrentDomain.GetData(key);
        if (previousLoadContext != null)
        {
            throw new Exception(
                $"ModuleInitializer called twice. Current AssemblyLoadContext: {loadContext.Name}. Previous AssemblyLoadContext: {previousLoadContext}");
        }

        AppDomain.CurrentDomain.SetData(key, loadContext.Name);
    }
}