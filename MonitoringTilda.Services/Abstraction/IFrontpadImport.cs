using System.Collections.Generic;
using System.Threading.Tasks;
using MonitoringTilda.Services.Models;

namespace MonitoringTilda.Services.Abstraction
{
    public interface IFrontpadImport
    {
        Task<string> AddOrders(IEnumerable<Order> orders);
    }
}