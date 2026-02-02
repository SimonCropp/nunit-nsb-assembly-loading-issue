using System.Reflection;
using System.Runtime.Loader;

public class EndToEndTests
{
    [Fact]
    public async Task Handler_in_custom_ALC_should_not_be_loaded_into_default_ALC()
    {
        var basePath = Path.GetDirectoryName(typeof(EndToEndTests).Assembly.Location)!;
        var customAlc = new AssemblyLoadContext("CustomALC", isCollectible: true);

        customAlc.Resolving += (context, name) =>
        {
            if (name.Name!.StartsWith("System") || name.Name.StartsWith("Microsoft"))
                return null;
            var path = Path.Combine(basePath, $"{name.Name}.dll");
            return File.Exists(path) ? context.LoadFromAssemblyPath(path) : null;
        };

        var assembly = customAlc.LoadFromAssemblyPath(Path.Combine(basePath, "HandlerAssembly.dll"));
        var runTest = assembly.GetType("HandlerAssembly.TestRunner")!.GetMethod("RunTest")!;

        var resetEvent = new AutoResetEvent(false);
        await (Task)runTest.Invoke(null, [resetEvent])!;

        var inDefault = AssemblyLoadContext.Default.Assemblies.Any(a => a.GetName().Name == "HandlerAssembly");
        customAlc.Unload();

        Assert.False(inDefault, "HandlerAssembly was loaded into Default ALC - this is the bug!");
    }
}
