using GoogleSheetsDatabase;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace GoogleSheetsDatabaseIntegrationTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.debug.json", true);

            var configuration = builder.Build();

            var sheetsDatabase = new GoogleSheetDatabase(
                spreadsheetId: configuration["spreadsheetId"],
                sheetsDatabaseClientEmail: configuration["sheetsDatabaseClientEmail"],
                sheetsDatabasePrivateKey: configuration["sheetsDatabasePrivateKey"]);

            Tester.TestAsync(sheetsDatabase).Wait();

            Console.ReadKey();
        }
    }
}
