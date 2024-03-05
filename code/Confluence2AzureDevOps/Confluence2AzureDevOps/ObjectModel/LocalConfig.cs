namespace Confluence2AzureDevOps.ObjectModel
{
    public class LocalMigrationConfig
    {
        /// <summary>
        /// Local directory path that contains the unzipped file that exported as HTML from Cloud Confluence.
        /// </summary>
        public string LocalConfluencePath { get; set; }

        /// <summary>
        /// Local directory path that contains migration result for upload to azure.
        /// </summary>
        public string LocalWorkspacePath { get; set; }
    }
}