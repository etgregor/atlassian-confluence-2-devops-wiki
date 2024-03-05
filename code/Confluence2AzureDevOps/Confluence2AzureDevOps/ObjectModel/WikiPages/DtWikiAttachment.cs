using Newtonsoft.Json;

namespace Confluence2AzureDevOps.ObjectModel.WikiPages
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
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Path of the wiki attachment file.
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }
    }
}