using AdvancedSharpAdbClient.Models;
using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Queries;

internal sealed class DevicesQuery : IAdbQuery<List<ShieldDevice>>
{
    public async Task<List<ShieldDevice>> ExecuteAsync(IAdbRunner runner)
    {
        var devices = await runner.QueryServerAsync((client, ct) => client.GetDevicesAsync(ct), []);

        return Map(devices);
    }

    public List<ShieldDevice> Map(IEnumerable<DeviceData> devices) =>
        devices.Select(d => new ShieldDevice(d.Serial, NullIfEmpty(d.Model), ToState(d.State))).ToList();

    private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;

    private static AdbDeviceState ToState(DeviceState state) => state switch
    {
        DeviceState.Online => AdbDeviceState.Online,
        DeviceState.Offline => AdbDeviceState.Offline,
        DeviceState.Unauthorized => AdbDeviceState.Unauthorized,
        _ => AdbDeviceState.Unknown,
    };
}
