namespace ShieldCommander.Core.Models;

public sealed record ThermalZone(string Name, double Value);

public sealed record ThermalSnapshot(List<ThermalZone> Zones);
