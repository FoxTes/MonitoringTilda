using System.Collections.Generic;

namespace MonitoringTilda.Services.Models
{
    public class Order
    {
        public List<SubOrder> SubOrders { get; set; }
        
        public string FirstName { get; set; }
        
        public string Phone { get; set; }
        
        public string Comment { get; set; }
        
        public int Price { get; set; }
        
        public string Date { get; set; }
        
        public SalesChannel Place { get; set; }
    }
}