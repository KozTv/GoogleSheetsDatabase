using GoogleSheetsDatabase;
using GoogleSheetsDatabaseIntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleSheetsDatabaseIntegrationTests
{
    static class Tester
    {

        public static async Task TestAsync(IGoogleSheetDatabase database)
        {
            DateTime startTime = DateTime.Now;

            for (int i = 0; i < 10; i++)
            {
                DateTime keyStartTime = DateTime.Now;

                var guid = Guid.NewGuid();
                await database.SetAsync("Sheet3", $"DEV Config_{i}", new ConfigurationData
                {
                    DictionaryProperty = new Dictionary<string, string>
                {
                    { "Key1", "Hello" },
                    { "Key2", "Value2" },
                    { "Key3", "Value3" }
                },
                    GuidProperty = guid,
                    IntProperty = i,
                    StringProperty = "My dev config"
                });

                var result = await database.GetAsync<ConfigurationData>("Sheet3", $"DEV Config_{i}");

                Console.WriteLine($"{i} write-read. {result.GuidProperty == guid} . {DateTime.Now - keyStartTime}");
            }

            Console.WriteLine($"Finished. Time taken: {DateTime.Now - startTime}");

        }

    }
}
