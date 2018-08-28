# GoogleSheetsDatabase
A small helper build on top of [Sheets API v4](https://developers.google.com/sheets/api/) which turns your Google Sheets into a key-value storage. 
Values are serialized to JSON to be human-readable and easily editable using standard Google Sheets web interface.

## Getting Started
Let's say we want to store some additional environment-specific configuration data and access it at runtime by environment name.
Define a model:
```c#
public class ConfigurationData
    {
        public int IntProperty { get; set; }
        public string StringProperty { get; set; }
        public Dictionary<string, string> DictionaryProperty { get; set; }
        public Guid GuidProperty { get; set; }
    }
```

Create an instance of GoogleSheetDatabase. You'll need `spreadsheetId`, `client Email` and `private key` (see [Prerequisites](#prerequisites) for details on how to obtain them):
```c#
var sheetsDatabase = new GoogleSheetDatabase(
    spreadsheetId: "1ax3TUtzIMbTBxFXnQ85lE91HoCVphB0-dnK-1USzd7c",
    sheetsDatabaseClientEmail: "sheetsdbtestserviceaccount@sheetsdbtestproject-214208.iam.gserviceaccount.com",
    sheetsDatabasePrivateKey: <YourPrivateKeyFromCreateServiceAccountKeyStep>);
```

Write a value:
```c#
var valueToSave = new ConfigurationData
    {
        DictionaryProperty = new Dictionary<string, string>
        {
            { "Key1", "Hello" },
            { "Key2", "Value2" }
        },
        GuidProperty = guid,
        IntProperty = 1,
        StringProperty = "My dev config 1"
    };

await sheetsDatabase.SetAsync("Sheet1", "DEV Config 1", valueToSave);
```

Read a value by key:

```c#
var result = await sheetsDatabase.GetAsync<ConfigurationData>("Sheet1", "DEV Config 1");
```

### Prerequisites

1. Create a new Google Sheet and copy its `spreadsheetId`.
1. Turn on the Google Sheets API.
   1. Visit [Google Developers Console](https://console.developers.google.com/project) and create a new project (or use an existing one).

   1. Enable Sheets API for your project.

   1. Create service account key for your project.

   1. Download JSON and copy private_key and client_email.
   
   1. Share your Google Sheet with the email from the previous step.

### Installing
Available at nuget:

    PM> Install-Package GoogleSheetsDatabase
