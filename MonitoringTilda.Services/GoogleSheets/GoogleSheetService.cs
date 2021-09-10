using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Options;
using MonitoringTilda.Services.Abstraction;
using MonitoringTilda.Services.ConfigModels;
using Newtonsoft.Json;

namespace MonitoringTilda.Services.GoogleSheets
{
    public class GoogleSheetService : IGoogleSheetService
    {
        private readonly Lazy<SheetsService> _sheetsService;
        private readonly ConcurrentBag<DataSheet> _dataSheetCache = new();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="GoogleSheetService"/>.
        /// </summary>
        /// <param name="configuration">Конфигурация.</param>
        public GoogleSheetService(IOptions<ApplicationConfigurationSettings> configuration)
        {
            var userCredential = new Lazy<ICredential>(() =>
            {
                var json = JsonConvert.SerializeObject(configuration.Value.GoogleServiceConfiguration.Credentials);
                return GoogleCredential.FromJson(json)
                    .CreateWithUser(configuration.Value.GoogleServiceConfiguration.UserLogin)
                    .CreateScoped(SheetsService.Scope.Spreadsheets)
                    .UnderlyingCredential;
            });

            _sheetsService = new Lazy<SheetsService>(() =>
                new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = userCredential.Value,
                ApplicationName = configuration.Value.ApplicationName
            }));
        }

        private SheetsService SheetsService => _sheetsService.Value;

        /// <inheritdoc />
        public async Task WriteValuesAsync(
            string spreadsheetId,
            CellRangeAddress rangeAddress,
            IList<IList<object>> values,
            int sheetId = 0)
        {
            var pageName = (await GetDataSheet(spreadsheetId, sheetId)).Title;
            var fullRange = $"'{pageName}'!{rangeAddress.Range}";
            var body = new ValueRange { Values = values };

            var updateRequest = SheetsService.Spreadsheets.Values.Update(body, spreadsheetId, fullRange);
            updateRequest.ValueInputOption =
                SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await updateRequest.ExecuteAsync();
        }

        /// <inheritdoc />
        public async Task<IList<IList<object>>> ReadValuesAsync(
            string spreadsheetId,
            CellRangeAddress rangeAddress,
            int sheetId = 0)
        {
            var pageName = (await GetDataSheet(spreadsheetId, sheetId)).Title;
            var fullRange = $"'{pageName}'!{rangeAddress.Range}";

            var readRequest = SheetsService.Spreadsheets.Values.Get(spreadsheetId, fullRange);
            var response = await readRequest.ExecuteAsync();
            return response.Values;
        }

        /// <inheritdoc />
        public async Task ClearSheetAsync(string spreadsheetId, int sheetId)
        {
            var pageName = (await GetDataSheet(spreadsheetId, sheetId)).Title;
            var fullRange = $"'{pageName}'!A1:Z";

            var updateRequest = SheetsService.Spreadsheets.Values
                .Clear(new ClearValuesRequest(), spreadsheetId, fullRange);
            await updateRequest.ExecuteAsync();
        }

        private async Task<DataSheet> GetDataSheet(string spreadsheetId, int sheetId)
        {
            var item = _dataSheetCache
                .FirstOrDefault(x => x.SpreadsheetId == spreadsheetId && x.SheetId == sheetId);
            if (item != null)
                return item;

            var readRequest = SheetsService.Spreadsheets.Get(spreadsheetId);
            var spreadsheet = await readRequest.ExecuteAsync();
            var title = spreadsheet.Sheets
                .FirstOrDefault(x => x.Properties.SheetId == sheetId)
                ?.Properties.Title;

            var newItem = new DataSheet
            {
                SpreadsheetId = spreadsheetId,
                SheetId = sheetId,
                Title = title
            };
            _dataSheetCache.Add(newItem);
            return newItem;
        }

        private class DataSheet
        {
            public string SpreadsheetId { get; init; }

            public int? SheetId { get; init; }

            public string Title { get; init; }
        }
    }
}