using NServiceBus;

namespace HandlerAssembly;

public class TestMessageHandler(AutoResetEvent @event) : IHandleMessages<TestMessage>
{
    public Task Handle(TestMessage message, IMessageHandlerContext context)
    {
        @event.Set();
        return Task.CompletedTask;
    }
}
