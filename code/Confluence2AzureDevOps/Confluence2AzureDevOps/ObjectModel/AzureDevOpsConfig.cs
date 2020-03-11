namespace Confluence2AzureDevOps.ObjectModel
{
    public class AzureDevOpsConfig
    {
        public string Organization { get; set; }

        public string Project { get; set; }

        public string WikiIdentifier { get; set; }

        public string PersonalAccessToken { get; set; }
    }
}