using System.Threading.Tasks;

namespace MonitoringTilda.Services.Abstraction
{
    public interface IMonitoringService
    {
        Task CheckNewOrders();
    }
}