using System.Runtime.CompilerServices;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Setup()
    {
        const string key = "ModuleInitializer_Initialized";
        if (AppDomain.CurrentDomain.GetData(key) != null)
        {
            throw new Exception("ModuleInitializer called twice");
        }
        AppDomain.CurrentDomain.SetData(key, true);
    }
}