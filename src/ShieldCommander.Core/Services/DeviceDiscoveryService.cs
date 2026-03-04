using ShieldCommander.Core.Models;
using Zeroconf;

namespace ShieldCommander.Core.Services;

public sealed class DeviceDiscoveryService : IDeviceDiscoveryService
{
    private static readonly TimeSpan ScanTimeout = TimeSpan.FromSeconds(10);
    
    // Shield advertises as Android TV remote service
    private static readonly string[] ServiceTypes =
    [
        "_androidtvremote2._tcp.local.",
        "_androidtvremote._tcp.local.",
    ];

    public async Task<IReadOnlyCollection<DiscoveredDevice>> ScanAsync(CancellationToken cancellationToken)
    {
        var results = await ZeroconfResolver.ResolveAsync(ServiceTypes, scanTime: ScanTimeout, cancellationToken: cancellationToken);

        return results.Select(host => new DiscoveredDevice(host.IPAddress, host.DisplayName)).ToList();
    }
}
