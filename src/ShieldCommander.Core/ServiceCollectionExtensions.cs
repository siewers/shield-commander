using Microsoft.Extensions.DependencyInjection;
using ShieldCommander.Core.Services;

namespace ShieldCommander.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShieldCommanderCore(this IServiceCollection services)
    {
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IAdbPathResolver, AdbPathResolver>();
        services.AddSingleton<IAdbPathProvider, AdbPathProvider>();
        services.AddSingleton<IAdbRunner, AdbRunner>();
        services.AddSingleton<IAdbExecutor, AdbExecutor>();
        services.AddSingleton<IDeviceDiscoveryService, DeviceDiscoveryService>();
        services.AddSingleton<IAdbConfigService, AdbConfigService>();
        services.AddSingleton<IDeviceConnectionService, DeviceConnectionService>();
        return services;
    }
}
