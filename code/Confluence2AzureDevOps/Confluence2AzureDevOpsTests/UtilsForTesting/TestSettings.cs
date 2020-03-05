namespace Confluence2AzureDevOpsTests.UtilsForTesting
{   
    /// <summary>
    /// Settings for azure devops
    /// </summary>
    public class DevOpsSettingsTests
    {
        public string Organization { get; set; }

        public string Project { get; set; }

        public string WikiIdentifier { get; set; }

        public string PersonalAccesToken { get; set; }
    }

    public class LocalSettingTests
    {
        public string LocalWikiSourceFolder { get; set; }
        
        public string LocalWikiDestinatioFolder { get; set; }
    }
}