using System.Collections.Generic;
using System.Threading.Tasks;
using MonitoringTilda.Services.GoogleSheets;

namespace MonitoringTilda.Services.Abstraction
{
    /// <summary>
    /// Определяет методы для работы с гугл таблицами.
    /// </summary>
    public interface IGoogleSheetService
    {
        /// <summary>
        /// Записывает данные в таблицу.
        /// </summary>
        /// <param name="spreadsheetId">Идентификатор таблицы.</param>
        /// <param name="rangeAddress">Диапазон записи.</param>
        /// <param name="values">Данные.</param>
        /// <param name="sheetId">Номер листа.</param>
        Task WriteValuesAsync(
            string spreadsheetId, 
            CellRangeAddress rangeAddress, 
            IList<IList<object>> values, 
            int sheetId);

        /// <summary>
        /// Читает данные из таблицы.
        /// </summary>
        /// <param name="spreadsheetId">Идентификатор таблицы.</param>
        /// <param name="rangeAddress">Диапазон чтения.</param>
        /// <param name="sheetId">Номер листа.</param>
        /// <returns>Прочитанные данные.</returns>
        Task<IList<IList<object>>> ReadValuesAsync(string spreadsheetId, CellRangeAddress rangeAddress, int sheetId);

        /// <summary>
        /// Удаляет все данные с листа.
        /// </summary>
        /// <param name="spreadsheetId">Идентификатор таблицы.</param>
        /// <param name="sheetId">Номер листа.</param>
        Task ClearSheetAsync(string spreadsheetId, int sheetId);
    }
}