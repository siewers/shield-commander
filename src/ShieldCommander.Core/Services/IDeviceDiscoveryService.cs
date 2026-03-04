using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services;

public interface IDeviceDiscoveryService
{
    Task<IReadOnlyCollection<DiscoveredDevice>> ScanAsync(CancellationToken cancellationToken);
}
