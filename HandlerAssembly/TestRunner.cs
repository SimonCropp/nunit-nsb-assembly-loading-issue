using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace HandlerAssembly;

public class TestMessage : IMessage;

public class TestMessageHandler(AutoResetEvent @event) : IHandleMessages<TestMessage>
{
    public Task Handle(TestMessage message, IMessageHandlerContext context)
    {
        @event.Set();
        return Task.CompletedTask;
    }
}

public class TestRunner
{
    public static async Task RunTest(AutoResetEvent resetEvent)
    {
        var configuration = new EndpointConfiguration("MinimalRepro");
        configuration.UseTransport<LearningTransport>();
        configuration.UsePersistence<LearningPersistence>();
        configuration.UseSerialization<SystemJsonSerializer>();
        configuration.RegisterComponents(_ => _.AddSingleton(resetEvent));

        var endpoint = await Endpoint.Start(configuration);
        await endpoint.SendLocal(new TestMessage());

        if (!resetEvent.WaitOne(TimeSpan.FromSeconds(10)))
            throw new Exception("Message not handled");

        await endpoint.Stop();
    }
}
