namespace Confluence2AzureDevOps.Entities.WikiPages
{
    /// <summary>
    /// Defines properties for wiki attachment file
    /// <see cref="https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/attachments/create?view=azure-devops-rest-5.1#wikiattachment"/>
    /// </summary>
    public class DtWikiAttachment
    {
        /// <summary>
        /// Name of the wiki attachment file.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Path of the wiki attachment file.
        /// </summary>
        public string path { get; set; }
    }
}