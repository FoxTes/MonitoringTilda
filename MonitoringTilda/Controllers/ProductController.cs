using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MonitoringTilda.Services.ConfigModels;
using MonitoringTilda.Services.Models;

namespace MonitoringTilda.Controllers
{
    /// <summary>
    /// Предоставляет данные для работы с продуктами.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        /// <summary>
        /// Получить все продукты, которые хранятся в кэше.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<HashSet<Product>> GetProducts(
            [FromServices] IMemoryCache memoryCache,
            [FromServices] IOptions<ApplicationConfigurationSettings> configurationSettings)
        {
            return await memoryCache.GetOrCreateAsync(
                configurationSettings.Value.ProductKey, _ => Task.FromResult(new HashSet<Product>()));
        }
        
        /// <summary>
        /// Получить все каналы, которые хранятся в кэше.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<HashSet<SalesChannel>> GetSalesChannel(
            [FromServices] IMemoryCache memoryCache,
            [FromServices] IOptions<ApplicationConfigurationSettings> configurationSettings)
        {
            return await memoryCache.GetOrCreateAsync(
                configurationSettings.Value.ChanelKey, _ => Task.FromResult(new HashSet<SalesChannel>()));
        }
    }
}