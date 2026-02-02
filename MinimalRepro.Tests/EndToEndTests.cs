public class EndToEndTests
{
    AutoResetEvent resetEvent = new(false);

    [Fact]
    public async Task Simple()
    {
        var configuration = new EndpointConfiguration("MinimalRepro");
        configuration.UseTransport<LearningTransport>();
        configuration.UsePersistence<LearningPersistence>();
        configuration.UseSerialization<SystemJsonSerializer>();
        configuration.RegisterComponents(_ => _.AddSingleton(resetEvent));

        var endpoint = await Endpoint.Start(configuration);
        await endpoint.SendLocal(new TestMessage());

        if (resetEvent.WaitOne(TimeSpan.FromSeconds(10)))
        {
            await endpoint.Stop();
            return;
        }

        await endpoint.Stop();
        throw new Exception("Message was not handled within timeout.");
    }

    public class TestMessage : IMessage;

    public class TestMessageHandler(AutoResetEvent @event) : IHandleMessages<TestMessage>
    {
        public Task Handle(TestMessage message, IMessageHandlerContext context)
        {
            @event.Set();
            return Task.CompletedTask;
        }
    }
}