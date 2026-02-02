using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace HandlerAssembly;

/// <summary>
/// This class simulates what happens in NUnit - test code that runs INSIDE the custom ALC
/// and calls NServiceBus from there.
/// </summary>
public class TestRunner
{
    public static async Task<bool> RunTest(AutoResetEvent resetEvent)
    {
        Console.WriteLine($"TestRunner executing in ALC: {System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(typeof(TestRunner).Assembly)?.Name}");

        var configuration = new EndpointConfiguration("MinimalRepro");
        configuration.UseTransport<LearningTransport>();
        configuration.UsePersistence<LearningPersistence>();
        configuration.UseSerialization<SystemJsonSerializer>();
        configuration.RegisterComponents(_ => _.AddSingleton(resetEvent));

        // NServiceBus will scan this assembly and find TestMessageHandler
        // Since we're running FROM the custom ALC, NServiceBus receives types from our ALC

        var endpoint = await Endpoint.Start(configuration);

        await endpoint.SendLocal(new TestMessage());

        var handled = resetEvent.WaitOne(TimeSpan.FromSeconds(10));

        await endpoint.Stop();

        return handled;
    }
}
