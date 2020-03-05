using NUnit.Framework;

namespace Confluence2AzureDevOpsTests.Utils
{
    public static class TestUtils
    {
        public static LocalSettingTests GetLocalSettings()
        {
            LocalSettingTests setting = new LocalSettingTests()
            {
                LocalWikiSourceFolder = TestContext.Parameters["localWikiSourceFolder"],
                LocalWikiDestinatioFolder = TestContext.Parameters["localWikiDestinatioFolder"]
            };
            
            return setting;
        }

        public static DevOpsSettingsTests GetDevopsTestSettings()
        {
            DevOpsSettingsTests settingsTests = new DevOpsSettingsTests()
            {
                Organization = TestContext.Parameters["organization"],

                Project = TestContext.Parameters["project"],

                WikiIdentifier = TestContext.Parameters["wikiIdentifier"],

                PersonalAccesToken = TestContext.Parameters["personalAccesToken"]
            };

            return settingsTests;
        } 
        
    }

    
    
    
    
    public class TestSettings
    {
        
        
//        _organization = TestContext.Parameters["organization"];
//            
//        _project = TestContext.Parameters["project"];
//            
//        _wikiIdentifier = TestContext.Parameters["wikiIdentifier"];
//            
//        _personalAccesToken = TestContext.Parameters["personalAccesToken"];
    }
}