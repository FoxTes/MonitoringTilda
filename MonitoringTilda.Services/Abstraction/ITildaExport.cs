using System.Collections.Generic;
using System.Threading.Tasks;
using MonitoringTilda.Services.Models;

namespace MonitoringTilda.Services.Abstraction
{
    public interface ITildaExport
    {
        Task GetProductIdAsync();
        
        Task GetSalesChannelIdAsync();

        Task<IEnumerable<Order>> GetOrderListAsync();
    }
}