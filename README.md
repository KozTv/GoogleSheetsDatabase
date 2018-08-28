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

After writing some data it will look like this in Google Sheets:
![2018-08-29_0144](https://user-images.githubusercontent.com/2159166/44755039-22c4c900-ab2d-11e8-9cbb-0ced19575a6e.png)

### Prerequisites

1. Create a new Google Sheet and copy its `spreadsheetId`:
![image4](https://user-images.githubusercontent.com/2159166/44754429-7c77c400-ab2a-11e8-83ec-198cb0acdf3f.png)
In the screenshot above `spreadsheetId=1ax3TUtzIMbTBxFXnQ85lE91HoCVphB0-dnK-1USzd7c`.
There is 1 sheet with name “Sheet1”. This represents a table “Sheet1”.

1. Turn on the Google Sheets API.
   1. Visit [Google Developers Console](https://console.developers.google.com/project) and create a new project (or use an existing one).
![image3](https://user-images.githubusercontent.com/2159166/44754428-7c77c400-ab2a-11e8-941f-e62c53a36a59.png)
   1. Enable Sheets API for your project.
![image5](https://user-images.githubusercontent.com/2159166/44754431-7d105a80-ab2a-11e8-9620-eb9bc2af5a84.png)
   1. Create service account key for your project.
![image6](https://user-images.githubusercontent.com/2159166/44754422-771a7980-ab2a-11e8-8e5a-1f47d5a54ffa.png)
   1. Download JSON and copy `private_key` and `client_email`.
   ![image2](https://user-images.githubusercontent.com/2159166/44754427-7bdf2d80-ab2a-11e8-9344-2cbca461779d.png)
   1. Share your Google Sheet with the email from the previous step.
![image1](https://user-images.githubusercontent.com/2159166/44754425-7bdf2d80-ab2a-11e8-9a94-b14caa381cd9.png)
### Installing
Available at nuget:

    PM> Install-Package GoogleSheetsDatabase
    
## Quotas

You can check quotas for your project in [Google Developers Console](https://console.developers.google.com/project).  
By default Google Sheets API sets the following limits:  
- Write requests per 100 seconds per user: 100  
- Read requests per 100 seconds per user: 100  
