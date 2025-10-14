using ThermoFisher.EventRouterModule.Contracts;

namespace ThermoFisher.SampleOnionModule.Abstractions;

public interface ITestMessagingService
{
    Task PublishEventRouterMessageAsync(EventRouterMessage msg);

    Task<int> AddAsync(int a, int b, CancellationToken cancellationToken);

    Task TestAsync();
}
