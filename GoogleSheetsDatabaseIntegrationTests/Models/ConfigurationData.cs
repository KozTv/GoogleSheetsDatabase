using System;
using System.Collections.Generic;

namespace GoogleSheetsDatabaseIntegrationTests.Models
{
    public class ConfigurationData
    {
        public int IntProperty { get; set; }
        public string StringProperty { get; set; }
        public Dictionary<string, string> DictionaryProperty { get; set; }
        public Guid GuidProperty { get; set; }
    }
}
