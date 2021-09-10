using System.Collections.Generic;

namespace MonitoringTilda.Services.GoogleSheets
{
    /// <summary>
    /// Конфигурация GoogleSheetApi.
    /// </summary>
    public class GoogleServiceConfiguration
    {
        /// <summary>
        /// Креды.
        /// </summary>
        public Dictionary<string, string> Credentials { get; set; }

        /// <summary>
        /// Логин/почта.
        /// </summary>
        public string UserLogin { get; set; }
    }
}