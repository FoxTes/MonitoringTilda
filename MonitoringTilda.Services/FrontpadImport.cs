using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MonitoringTilda.Services.Abstraction;
using MonitoringTilda.Services.ConfigModels;
using MonitoringTilda.Services.Models;

namespace MonitoringTilda.Services
{
    public class FrontpadImport : IFrontpadImport
    {
        private readonly ApplicationConfigurationSettings _configurationSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public FrontpadImport(
            IOptions<ApplicationConfigurationSettings> configurationSettings,
            IHttpClientFactory httpClientFactory)
        {
            _configurationSettings = configurationSettings.Value;
            _httpClientFactory = httpClientFactory;
        }
        
        public async Task<string> AddOrders(IEnumerable<Order> orders)
        {
            var result = string.Empty;
            foreach (var order in orders)
            {
                result += AddOrder(order);
                await Task.Delay(2500);
            }

            return result;
        }

        private async Task<string> AddOrder(Order order)
        {
            var parameters = new List<KeyValuePair<string, string>>
                { new("secret", _configurationSettings.FrontpadSecretKey) };
            
            foreach (var subOrder in order.SubOrders)
            {
                parameters.Add(new KeyValuePair<string, string>("product[]", subOrder.Id));
                parameters.Add(new KeyValuePair<string, string>("product_kol[]", subOrder.Count.ToString()));
            }
            parameters.Add(new KeyValuePair<string, string>("phone", order.Phone));
            parameters.Add(new KeyValuePair<string, string>("name", order.FirstName));
            parameters.Add(new KeyValuePair<string, string>("descr", $"{order.Comment} Место: {order.Place.Name}"));
            parameters.Add(new KeyValuePair<string, string>("channel", order.Place.Id));
            
            var dataContent = new FormUrlEncodedContent(parameters.ToArray());

            var httpclient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpclient.PostAsync(_configurationSettings.FrontpadCommandUrl, dataContent);
            var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

            return $"Код - {httpResponseMessage.StatusCode} + {responseContent}";
        }
    }
}