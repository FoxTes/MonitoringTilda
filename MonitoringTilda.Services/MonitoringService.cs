using System.Threading.Tasks;
using MonitoringTilda.Services.Abstraction;

namespace MonitoringTilda.Services
{
    public class MonitoringService : IMonitoringService
    {
        private readonly ITildaExport _tildaExport;
        private readonly IFrontpadImport _frontpadImport;

        public MonitoringService(
            ITildaExport tildaExport, 
            IFrontpadImport frontpadImport)
        {
            _tildaExport = tildaExport;
            _frontpadImport = frontpadImport;
        }
        
        public async Task CheckNewOrders()
        {
            var orders = await _tildaExport.GetOrderListAsync();
            if (orders != null) 
                await _frontpadImport.AddOrders(orders);
        }
    }
}