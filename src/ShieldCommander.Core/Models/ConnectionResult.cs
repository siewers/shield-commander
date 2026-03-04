namespace ShieldCommander.Core.Models;

public sealed record ConnectionResult(ConnectionStatus Status, string? DeviceName = null);
