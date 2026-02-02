// using System.Runtime.CompilerServices;
// using System.Runtime.Loader;
//
// namespace HandlerAssembly;
//
// public static class ModuleInitializer
// {
//     [ModuleInitializer]
//     public static void Setup()
//     {
//         var alc = AssemblyLoadContext.GetLoadContext(typeof(ModuleInitializer).Assembly)!;
//         Console.WriteLine($"HandlerAssembly loaded in ALC: {alc.Name}");
//
//         const string key = "HandlerAssembly_LoadedInALCs";
//         var loaded = (HashSet<string>?)AppDomain.CurrentDomain.GetData(key) ?? [];
//
//         if (!loaded.Add(alc.Name!))
//             throw new Exception($"HandlerAssembly loaded twice in same ALC: {alc.Name}");
//
//         AppDomain.CurrentDomain.SetData(key, loaded);
//     }
// }
