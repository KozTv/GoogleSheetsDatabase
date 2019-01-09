using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GoogleSheetsDatabase
{
    public class GoogleSheetDatabase : IDisposable, IGoogleSheetDatabase
    {
        SheetsService _service;
        private readonly string _spreadsheetId;
        private readonly IDictionary<string, int> _sheetIds;
        private static readonly string[] _scopes = { SheetsService.Scope.Spreadsheets };
        private static readonly string _appendRange = "A1:C1";

        public GoogleSheetDatabase(string spreadsheetId, string sheetsDatabaseClientEmail, string sheetsDatabasePrivateKey)
        {
            try
            {
                ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(sheetsDatabaseClientEmail)
               {
                   Scopes = _scopes
               }.FromPrivateKey(sheetsDatabasePrivateKey));

                // Create Google Sheets API service.
                _service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                });

                _spreadsheetId = spreadsheetId;
                var spreadsheetInfo = _service.Spreadsheets.Get(spreadsheetId).Execute();

                _sheetIds = spreadsheetInfo.Sheets.Select(x => new
                {
                    Id = x.Properties.SheetId,
                    Name = x.Properties.Title
                }).ToDictionary(x => x.Name, y => y.Id.Value);
            }
            catch
            {
                Dispose();
                throw;
            }

        }

        public async Task<T> GetAsync<T>(string table, string uId)
        {
            string rawData = await GetStringAsync(table, uId);
            if (!string.IsNullOrEmpty(rawData))
            {
                return JsonConvert.DeserializeObject<T>(rawData);
            }

            return default(T);

        }

        private async Task<string> GetStringAsync(string table, string uId)
        {
            string range = A1NotationByTableAndUid(table, uId);
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    _service.Spreadsheets.Values.Get(_spreadsheetId, range);

            try
            {
                ValueRange response = await request.ExecuteAsync();
                return response.Values.FirstOrDefault()?[1].ToString();
            }
            catch (GoogleApiException ex)
            {
                if (ex.Message.Contains("Unable to parse range"))
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

        }

        public async Task SetAsync<T>(string table, string uId, T value)
        {
            string range = A1NotationByTableAndUid(table, uId);
            ValueRange values = CreateValueRange(uId, JsonConvert.SerializeObject(value));

            try
            {
                SpreadsheetsResource.ValuesResource.UpdateRequest request =
                        _service.Spreadsheets.Values.Update(values, _spreadsheetId, range);
                request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

                var response = await request.ExecuteAsync();
            }
            catch (GoogleApiException ex)
            {
                if (ex.Message.Contains("Unable to parse range"))
                {
                    await InsertInternal(table, uId, values, false);
                }
                else
                {
                    throw;
                }
            }
        }

        public Task DeleteAsync(string table, string uId)
        {
            throw new NotImplementedException();
        }

        private string A1NotationByTableAndUid(string table, string uId)
        {
            return $"{table}!{GetNamedRangeNameForKey(table, uId)}";
        }

        private string A1NotationByTableAndA1Range(string table, string a1Range)
        {
            return $"{table}!{a1Range}";
        }

        private async Task InsertInternal(string table, string uId, ValueRange values, bool checkPk)
        {
            var range = A1NotationByTableAndA1Range(table, _appendRange);

            if (checkPk)
            {
                // kind of primary key constraint check
                if (await GetStringAsync(table, uId) != null)
                {
                    throw new InvalidOperationException($"Key {uId} already exist in table {table}");
                }
            }

            SpreadsheetsResource.ValuesResource.AppendRequest request =
                    _service.Spreadsheets.Values.Append(values, _spreadsheetId, range);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            var response = await request.ExecuteAsync();

            ApplyNameToRange(table, response.Updates.UpdatedRange, GetNamedRangeNameForKey(table, uId));
        }

        private static ValueRange CreateValueRange(string uId, string value)
        {
            return new ValueRange()
            {
                MajorDimension = "ROWS",
                Values = new List<IList<object>>
                {
                    new List<object>
                    {
                        uId,
                        value,
                        DateTimeOffset.UtcNow
                    }
                }
            };
        }

        private GridRange A1RangeToGridRange(string tableName, string a1Range)
        {
            string a1 = a1Range.Substring(tableName.Length + 1);
            string row = a1.Substring(a1.LastIndexOf(":") + 2);
            int rowIndex = int.Parse(row);
            return new GridRange
            {
                SheetId = _sheetIds[tableName],
                StartColumnIndex = 0,
                EndColumnIndex = 3,
                StartRowIndex = rowIndex - 1,
                EndRowIndex = rowIndex
            };

        }

        private string GetNamedRangeNameForKey(string table, string primaryKey)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(_sheetIds[table] + primaryKey);
            SHA256 hashstring = SHA256.Create();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hex = BitConverter.ToString(hash);
            return $"SHA{hex.Replace("-", "")}";
        }

        private void ApplyNameToRange(string table, string range, string name)
        {
            var request = _service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request> {
                    new Request
                    {
                        AddNamedRange = new AddNamedRangeRequest
                        {
                            NamedRange = new NamedRange
                            {
                                Name = name,
                                Range = A1RangeToGridRange(table, range)
                            }
                        }
                    }

                }
            }, _spreadsheetId);

            request.Execute();
        }

        public void Dispose()
        {
            if (_service != null)
            {
                _service.Dispose();
            }
        }
    }
}
