using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MonitoringTilda.Services.Abstraction;
using MonitoringTilda.Services.ConfigModels;
using MonitoringTilda.Services.Extensions;

namespace MonitoringTilda
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ApplicationConfigurationSettings>(options =>
                Configuration.GetSection(nameof(ApplicationConfigurationSettings)).Bind(options));
            
            services.AddServices();
            
            services.AddControllers();

            services.AddHttpClient();

            services.AddMemoryCache();
            
            services.AddHangfire(config => config.UseMemoryStorage());
            services.AddHangfireServer();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MonitoringTilda", Version = "v1" });
            });
        } 
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MonitoringTilda v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            
            app.UseHangfireDashboard();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard();
            });
            
            var getSalesJobId = BackgroundJob.Enqueue<ITildaExport>(
                service => service.GetSalesChannelIdAsync());
            var getProductJobId = BackgroundJob.ContinueJobWith<ITildaExport>(getSalesJobId, 
                service => service.GetProductIdAsync());
            BackgroundJob.ContinueJobWith<IMonitoringService>(getProductJobId, service => service.CheckNewOrders());

            RecurringJob.AddOrUpdate<IMonitoringService>(
                "Проверка на наличие новых заказов",
                service => service.CheckNewOrders(),
                "* * * * *");
        }
    }
}