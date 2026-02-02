using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace HandlerAssembly;

public static class ModuleInitializer
{
    const string InitializedContextsKey = "HandlerAssembly_ModuleInitializer_InitializedContexts";
    static readonly object lockObj = new();

    [ModuleInitializer]
    public static void Setup()
    {
        var loadContext = AssemblyLoadContext.GetLoadContext(typeof(ModuleInitializer).Assembly)!;
        var contextName = loadContext.Name ?? "unnamed";

        Console.WriteLine($"=== HandlerAssembly ModuleInitializer called in ALC: {contextName} ===");

        lock (lockObj)
        {
            var initializedContexts = (HashSet<string>?)AppDomain.CurrentDomain.GetData(InitializedContextsKey);
            if (initializedContexts == null)
            {
                initializedContexts = [];
                AppDomain.CurrentDomain.SetData(InitializedContextsKey, initializedContexts);
            }

            if (initializedContexts.Contains(contextName))
            {
                throw new Exception(
                    $"HandlerAssembly ModuleInitializer called twice in SAME AssemblyLoadContext: {contextName}\n" +
                    $"Stack trace:\n{Environment.StackTrace}");
            }

            initializedContexts.Add(contextName);
            Console.WriteLine($"Total ALCs that have loaded HandlerAssembly: {initializedContexts.Count} ({string.Join(", ", initializedContexts)})");
        }
    }
}
