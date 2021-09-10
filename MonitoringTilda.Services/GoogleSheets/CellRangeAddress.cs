using System.Collections.Generic;
using System.Linq;

namespace MonitoringTilda.Services.GoogleSheets
{
    /// <summary>
    /// Адрес диапазона ячеек.
    /// </summary>
    public class CellRangeAddress
    {
        /// <summary>
        /// Создает новый диапазон ячеек с данными.
        /// </summary>
        /// <param name="startColumnIndex">Индекс первого столбца.</param>
        /// <param name="startRowIndex">Индекс первой строки.</param>
        /// <param name="values">Данные для записи.</param>
        public CellRangeAddress(
            ICollection<IList<object>> values,
            int startColumnIndex = 0,
            int startRowIndex = 0)
        {
            StartColumnIndex = startColumnIndex;
            StartRowIndex = startRowIndex;
            EndColumnIndex = startColumnIndex + (values.First()?.Count - 1 ?? 0);
            EndRowIndex = startRowIndex + values.Count - 1;
        }

        /// <summary>
        /// Создает новый диапазон ячеек для чтения.
        /// </summary>
        /// <param name="startColumnIndex">Индекс первого столбца.</param>
        /// <param name="endColumnIndex">Индекс последнего столбца (включительно).</param>
        /// <param name="startRowIndex">Индекс первой строки.</param>
        /// <param name="endRowIndex">Индекс последней строки (включительно).</param>
        /// <remarks>
        /// Индекс начинается с нулевого значения.<br/>
        /// По умолчанию читается ячейка A1.
        /// </remarks>
        public CellRangeAddress(
            int startColumnIndex = 0,
            int endColumnIndex = 0,
            int? startRowIndex = null,
            int? endRowIndex = null)
        {
            StartColumnIndex = startColumnIndex;
            StartRowIndex = startRowIndex;
            EndColumnIndex = endColumnIndex;
            EndRowIndex = endRowIndex;
        }

        /// <summary>
        /// Возвращает диапазон ячеек в формате A1-нотации.
        /// </summary>
        public string Range => GetRange();

        private int StartColumnIndex { get; }

        private int? StartRowIndex { get; }

        private int EndColumnIndex { get; }

        private int? EndRowIndex { get; }

        private static string IntegerToLetters(int columnNumber)
        {
            var columnName = string.Empty;

            columnNumber += 1;
            while (columnNumber > 0)
            {
                var rem = columnNumber % 26;
                if (rem == 0)
                {
                    columnName += "Z";
                    columnNumber = columnNumber / 26 - 1;
                }
                else
                {
                    columnName += (char)(rem - 1 + 'A');
                    columnNumber /= 26;
                }
            }

            return new string(columnName.Reverse().ToArray());
        }

        private string GetRange()
        {
            var startRowIndex = StartRowIndex.HasValue ? (StartRowIndex + 1).ToString() : string.Empty;
            var endRowIndex = EndRowIndex.HasValue ? (EndRowIndex + 1).ToString() : string.Empty;

            var startReference = $"{IntegerToLetters(StartColumnIndex)}{startRowIndex}";
            var endReference = $"{IntegerToLetters(EndColumnIndex)}{endRowIndex}";

            return endReference != startReference
                   || startReference.All(c => !char.IsLetter(c))
                   || startReference.All(c => !char.IsDigit(c))
                ? $"{startReference}:{endReference}"
                : startReference;
        }
    }
}