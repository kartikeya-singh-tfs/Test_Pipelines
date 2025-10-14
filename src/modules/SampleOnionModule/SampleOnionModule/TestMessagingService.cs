using Microsoft.Extensions.Logging;
using SampleOnionModule.Contracts;
using ThermoFisher.EventRouterModule.Contracts;
using ThermoFisher.SampleOnionModule.Abstractions;
using Wolverine;

namespace ThermoFisher.SampleOnionModule;

public class TestMessagingService(IMessageBus messageBus, ILogger<TestMessagingService> logger)
    : ITestMessagingService
{
    public async Task<int> AddAsync(int a, int b, CancellationToken cancellationToken)
    {
        var sum = await messageBus.InvokeAsync<AddResponse>(
            new AddRequest(5, 10),
            cancellationToken
        );

        logger?.LogInformation("AddResponse: {Sum}", sum?.Sum);

        return sum?.Sum ?? 0;
    }

    public async Task PublishEventRouterMessageAsync(EventRouterMessage msg)
    {
        await messageBus.PublishAsync(msg);
    }

    public async Task TestAsync()
    {
        await messageBus.SendAsync(new SendRequestMessage(1));
        await messageBus.PublishAsync(new SendRequestMessage(2));
        await messageBus.PublishAsync(new PublishRequestMessage(3));
        try
        {
            await messageBus.SendAsync(new PublishRequestMessage(4));
        }
        catch (Exception ex)
        {
            // Expected exception: Wolverine.Runtime.Routing.IndeterminateRoutesException
            // As reference to Wolverine is not added to this module, catching Exception

            // IndeterminateRoutesException exception is expected because there is no handler for PublishRequestMessage defined and SendAsync is called.

            logger?.LogInformation(
                $"SendAsync(PublishRequestMessage) failed as expected: {ex.Message}"
            );
        }
    }
}
