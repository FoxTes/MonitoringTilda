using MonitoringTilda.Services.GoogleSheets;

namespace MonitoringTilda.Services.ConfigModels
{
    /// <summary>
    /// Конфигурация приложения.
    /// </summary>
    public class ApplicationConfigurationSettings
    {
        /// <summary>
        /// Ссылка на сайт проекта.
        /// </summary>
        public string TildaUrl { get; set; }

        /// <summary>
        /// Ключ продукта для хранения в кэше.
        /// </summary>
        public string ProductKey { get; set; }
        
        /// <summary>
        /// Ключ канала для хранения в кэше.
        /// </summary>
        public string ChanelKey { get; set; }
        
        /// <summary>
        /// Адрес запроса на добавление заказа.
        /// </summary>
        public string FrontpadCommandUrl { get; set; }
        
        /// <summary>
        /// Секрет для доступа к Frontpad.
        /// </summary>
        public string FrontpadSecretKey { get; set; }
        
        /// <summary>
        /// Конфигурация GoogleSheetApi.
        /// </summary>
        public GoogleServiceConfiguration GoogleServiceConfiguration { get; set; }

        /// <summary>
        /// Название приложения в GoogleSheet.
        /// </summary>
        public string ApplicationName { get; set; }
        
        /// <summary>
        /// Идентификатор листа для выгрузки.
        /// </summary>
        public int ExportSheetSalesChannelId { get; set; }

        /// <summary>
        /// Идентификатор гугл таблицы.
        /// </summary>
        public string SpreadsheetSalesChannelId { get; set; }

        /// <summary>
        /// Идентификатор листа продуктов для выгрузки.
        /// </summary>
        public int ExportSheetProductId { get; set; }

        /// <summary>
        /// Идентификатор гугл таблицы для продуктов.
        /// </summary>
        public string SpreadsheetProductId { get; set; }
    }
}