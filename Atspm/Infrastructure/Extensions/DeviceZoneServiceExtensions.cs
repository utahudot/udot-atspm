using Microsoft.Extensions.DependencyInjection;
using Utah.Udot.Atspm.Services;

namespace Utah.Udot.Atspm.Infrastructure.Extensions;

public static class DeviceZoneServiceExtensions
{
    public static IServiceCollection AddDeviceZoneProviders(this IServiceCollection services)
    {
        services.AddHttpClient("DeviceZones").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false });
        services.RegisterServicesByInterface<IDeviceZoneProvider>();
        return services;
    }
}
