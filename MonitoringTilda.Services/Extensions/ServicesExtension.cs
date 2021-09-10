using Microsoft.Extensions.DependencyInjection;
using MonitoringTilda.Services.Abstraction;
using MonitoringTilda.Services.GoogleSheets;

namespace MonitoringTilda.Services.Extensions
{
    public static class ServicesExtension
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IGoogleSheetService, GoogleSheetService>();
            services.AddSingleton<ITildaExport, TildaExport>();
            services.AddSingleton<IFrontpadImport, FrontpadImport>();
            services.AddSingleton<IMonitoringService, MonitoringService>();
            
            return services;
        }
    }
}