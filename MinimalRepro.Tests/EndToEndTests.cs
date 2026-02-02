using System.Reflection;
using System.Runtime.Loader;

public class EndToEndTests
{
    [Fact]
    public async Task Should_not_fail_when_endpoint_started_from_custom_AssemblyLoadContext()
    {
        // Simulate what NUnit Engine does:
        // 1. Create a custom ALC named after the test assembly
        // 2. Load the test assembly into it
        // 3. Execute test code FROM WITHIN the custom ALC (this is the key!)

        var basePath = Path.GetDirectoryName(typeof(EndToEndTests).Assembly.Location)!;

        // Create ALC named after the handler assembly (like NUnit does with test assemblies)
        var customAlc = new AssemblyLoadContext("HandlerAssembly", isCollectible: true);

        // Register resolver for dependencies (like NUnit's TestAssemblyResolver)
        customAlc.Resolving += (context, assemblyName) =>
        {
            // Skip system assemblies - let them resolve from Default
            if (assemblyName.Name != null &&
                (assemblyName.Name.StartsWith("System") ||
                 assemblyName.Name.StartsWith("Microsoft") ||
                 assemblyName.Name.StartsWith("netstandard") ||
                 assemblyName.Name == "mscorlib"))
            {
                return null;
            }

            // Try to load from base path (like NUnit does)
            var assemblyPath = Path.Combine(basePath, $"{assemblyName.Name}.dll");
            if (File.Exists(assemblyPath))
            {
                Console.WriteLine($"CustomALC resolving: {assemblyName.Name}");
                return context.LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        };

        // Load the handler assembly into the custom ALC (like NUnit loads test assembly)
        var handlerAssemblyPath = Path.Combine(basePath, "HandlerAssembly.dll");
        var loadedAssembly = customAlc.LoadFromAssemblyPath(handlerAssemblyPath);

        Console.WriteLine($"Loaded HandlerAssembly into ALC: {AssemblyLoadContext.GetLoadContext(loadedAssembly)?.Name}");

        // Get TestRunner type from the custom ALC
        var testRunnerType = loadedAssembly.GetType("HandlerAssembly.TestRunner")!;

        // Verify it's in custom ALC
        Assert.Equal("HandlerAssembly", AssemblyLoadContext.GetLoadContext(testRunnerType.Assembly)?.Name);

        var resetEvent = new AutoResetEvent(false);

        // NOW THE KEY PART: Invoke the test runner FROM the custom ALC
        // This simulates NUnit executing test code from within its custom ALC
        var runTestMethod = testRunnerType.GetMethod("RunTest", BindingFlags.Public | BindingFlags.Static)!;

        Console.WriteLine("Invoking TestRunner.RunTest from custom ALC...");

        // This invocation simulates what NUnit does - calling test code that's loaded in a custom ALC
        var task = (Task<bool>)runTestMethod.Invoke(null, [resetEvent])!;
        var handled = await task;

        // Check if HandlerAssembly was loaded into Default ALC (this is the bug)
        var defaultAssemblies = AssemblyLoadContext.Default.Assemblies
            .Select(a => a.GetName().Name)
            .ToList();

        Console.WriteLine($"Assemblies in Default ALC: {string.Join(", ", defaultAssemblies.Where(n => n != null && !n.StartsWith("System") && !n.StartsWith("Microsoft")))}");

        customAlc.Unload();

        if (defaultAssemblies.Contains("HandlerAssembly"))
        {
            throw new Exception(
                "BUG REPRODUCED: HandlerAssembly was loaded into Default ALC!\n" +
                "This is the same issue seen with NUnit - when code running in a custom ALC " +
                "calls NServiceBus, the generic method invocation causes the assembly to be " +
                "loaded into the Default AssemblyLoadContext.");
        }

        Assert.True(handled, "Message was not handled within timeout.");
    }
}
