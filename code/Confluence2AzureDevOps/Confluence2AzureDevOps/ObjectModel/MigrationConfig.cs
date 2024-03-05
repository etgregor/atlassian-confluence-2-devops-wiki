using System.Collections.Generic;

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

        /// <summary>
        /// Because titles can be so long and on Azure Wiki creates tree, you can replace titles with other short or abbreviation.
        /// </summary>
        public Dictionary<string, string> ReplazableTitles { get; set; }
    }
}