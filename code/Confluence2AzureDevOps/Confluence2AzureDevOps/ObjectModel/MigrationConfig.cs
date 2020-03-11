namespace Confluence2AzureDevOps.ObjectModel
{
    public class MigrationConfig
    {
        public MigrationConfig()
        {
            LocalConfig = new LocalMigrationConfig();
            
            AzureDevOpsConfig = new AzureDevOpsConfig();
        }
        
        public LocalMigrationConfig LocalConfig { get; set; }

        public AzureDevOpsConfig AzureDevOpsConfig { get; set; }
    }
}