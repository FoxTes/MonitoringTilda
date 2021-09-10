using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MonitoringTilda.Services.Abstraction;
using MonitoringTilda.Services.ConfigModels;
using MonitoringTilda.Services.GoogleSheets;
using MonitoringTilda.Services.Helpers;
using MonitoringTilda.Services.Models;

namespace MonitoringTilda.Services
{
    public class TildaExport : ITildaExport
    {
        private readonly ApplicationConfigurationSettings _configurationSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly IGoogleSheetService _googleSheetService;

        public TildaExport(
            IOptions<ApplicationConfigurationSettings> configurationSettings,
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache,
            IGoogleSheetService googleSheetService)
        {
            _configurationSettings = configurationSettings.Value;
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
            _googleSheetService = googleSheetService;
        }
        
        public async Task GetProductIdAsync()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var httpResult = await httpClient.GetStringAsync(
                $"{_configurationSettings.TildaUrl}/feed-fb.csv", 
                CancellationToken.None);

            var result = httpResult
                .Split(Environment.NewLine.ToCharArray())
                .Select(x => x.Split(","))
                .Skip(1)
                .Where(x => x.Length > 2)
                .Select(x => new Product {Id = x[0], Name = x[1].Trim('"')})
                .ToList();
            
            var productCache = await _memoryCache.GetOrCreateAsync(
                _configurationSettings.ProductKey, _ => Task.FromResult(new HashSet<Product>()));

            foreach (var item in result)
            {
                if (item.Name.Contains("см", StringComparison.CurrentCultureIgnoreCase))
                {
                    var index = item.Name.IndexOf("-", StringComparison.Ordinal) + 1;
                    var newName = item.Name[..(index - 1)];
                    item.Name = newName + $"(Размер:{item.Name.Substring(index, 3)}см)";
                }
                
                productCache.Add(item);
            }
        }

        public async Task GetSalesChannelIdAsync()
        {
            var valuesSalesChannel = await _googleSheetService.ReadValuesAsync(
                _configurationSettings.SpreadsheetSalesChannelId,
                new CellRangeAddress(0, 2, 1),
                _configurationSettings.ExportSheetSalesChannelId);

            var channels = valuesSalesChannel
                .Select(x => new SalesChannel
                {
                Id = (string)x[0],
                Name = (string)x[1],
                Alias = (string)x[2],
            });
            
            var salesChannelCache = await _memoryCache.GetOrCreateAsync(
                _configurationSettings.ChanelKey, _ => Task.FromResult(new HashSet<SalesChannel>()));

            foreach (var channel in channels) 
                salesChannelCache.Add(channel);
        }

        public async Task<IEnumerable<Order>> GetOrderListAsync()
        {
            var valuesParameters = await _googleSheetService.ReadValuesAsync(
                _configurationSettings.SpreadsheetProductId,
                new CellRangeAddress(0, 32, 0, 0),
                _configurationSettings.ExportSheetProductId);
            if (valuesParameters == null)
                throw new Exception("Не найдены параметры в гугл таблице.");
            var parameters = valuesParameters
                .First()
                .Cast<string>()
                .ToList();

            IList<IList<object>> valuesOrders;

            var numberLastSheet = int.Parse(Settings.ReadAppSetting().NumberLastRow);
            if (numberLastSheet == 0)
            {
                valuesOrders = await _googleSheetService.ReadValuesAsync(
                    _configurationSettings.SpreadsheetProductId,
                    new CellRangeAddress(0, parameters.Count, 1),
                    _configurationSettings.ExportSheetProductId);
                Settings.AddOrUpdateAppSetting("NumberLastRow", (valuesOrders.Count + 1).ToString() );
            }
            else
            {
                valuesOrders = await _googleSheetService.ReadValuesAsync(
                    _configurationSettings.SpreadsheetProductId,
                    new CellRangeAddress(0, parameters.Count, numberLastSheet),
                    _configurationSettings.ExportSheetProductId);
                
                if (valuesOrders != null)
                    Settings.AddOrUpdateAppSetting("NumberLastRow", (numberLastSheet + valuesOrders.Count).ToString());
            }
            
            var productsCache = await _memoryCache.GetOrCreateAsync(
                _configurationSettings.ProductKey, _ => Task.FromResult(new HashSet<Product>()));
            
            var salesChannelCache = await _memoryCache.GetOrCreateAsync(
                _configurationSettings.ChanelKey, _ => Task.FromResult(new HashSet<SalesChannel>()));

            return valuesOrders?.Select(x => new Order
            {
                FirstName = (string)x[0],
                Phone = (string)x[1],
                Comment = (string)x[2],
                SubOrders = ParseOrder((string)x[4]),
                Price = Convert.ToInt32(x[5]),
                Date = (string)x[8],
                Place = x.Count >= 12 ? ParsePlace((string)x[11]) : null
            });
            
            List<SubOrder> ParseOrder(string data)
            {
                return data
                    .Split(";")
                    .Select(x => new SubOrder
                    {
                        Id = MatchingId(x),
                        Name = x,
                        Count = MatchingCount(x)
                    })
                    .ToList();

                string MatchingId(string name)
                {
                    var index = name.IndexOf("-", StringComparison.Ordinal) + 1;
                    var parseName = name[..(index - 2)].Trim();

                    foreach (var product in productsCache.Where(product => parseName == product.Name))
                        return product.Id;

                    return string.Empty;
                }
                
                int MatchingCount(string name)
                {
                    var indexStart = name.IndexOf("-", StringComparison.Ordinal) + 2;
                    var indexStop = name.IndexOf("x", StringComparison.Ordinal) - 1;

                    return Convert.ToInt32(indexStart == indexStop 
                        ? name.Substring(indexStart, 1) 
                        : name.Substring(indexStart, indexStop-indexStart + 1));
                }
            }

            SalesChannel ParsePlace(string data) 
                => salesChannelCache.FirstOrDefault(x => x.Alias == data);
        }
    }
}