namespace ShieldCommander.Core.Models;

public sealed record ShieldDevice(string IpAddress, string? DeviceName = null, AdbDeviceState State = AdbDeviceState.Online);
