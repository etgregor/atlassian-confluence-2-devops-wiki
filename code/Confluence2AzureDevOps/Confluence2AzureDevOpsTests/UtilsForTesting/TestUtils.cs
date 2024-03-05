using System;
using System.Text;
using Confluence2AzureDevOps.ObjectModel;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Confluence2AzureDevOpsTests.UtilsForTesting
{
    public static class TestUtils
    {
        public static MigrationConfig GetMigrationConfigTest()
        {            
            string filePath = TestContext.Parameters["Confluence2DevopsFileConfigPath"];

            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("Require config file.");
            }

            string configAsJson = System.IO.File.ReadAllText(filePath, Encoding.UTF8);

            MigrationConfig fileContent = JsonConvert.DeserializeObject<MigrationConfig>(configAsJson);

            return fileContent;
        }
    }
}