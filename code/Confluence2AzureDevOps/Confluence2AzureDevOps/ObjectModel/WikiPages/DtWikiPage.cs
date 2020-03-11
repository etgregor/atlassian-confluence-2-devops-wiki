using System.Collections.Generic;
using Newtonsoft.Json;

namespace Confluence2AzureDevOps.ObjectModel.WikiPages
{
    /// <summary>
    /// Defines a page in a wiki.
    /// See more <see cref="https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages/get%20page?view=azure-devops-rest-5.1#wikipage">here</see>.
    /// </summary>
    public abstract class DtWikiPage
    {
        /// <summary>
        /// Content of the wiki page.
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// Path of the git item corresponding to the wiki page stored in the backing Git repository.
        /// </summary>
        [JsonProperty("GitItemPath")]
        public string GitItemPath { get; set; }

        /// <summary>
        /// When present, permanent identifier for the wiki page
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// True if a page is non-conforming, i.e. 1) if the name doesn't match page naming standards. 2) if the page does not have a valid entry in the appropriate order file.
        /// </summary>
        [JsonProperty("isNonConformant")]
        public bool IsNonConformant { get; set; }

        /// <summary>
        /// True if this page has subpages under its path.
        /// </summary>
        [JsonProperty("isParentPage")]
        public bool IsParentPage { get; set; }

        /// <summary>
        /// Order of the wiki page, relative to other pages in the same hierarchy level.
        /// </summary>
        [JsonProperty("order")]
        public int Order { get; set; }

        /// <summary>
        /// Path of the wiki page.
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        /// Remote web url to the wiki page.
        /// </summary>
        [JsonProperty("remoteUrl")]
        public string RemoteUrl { get; set; }

        /// <summary>
        /// List of subpages of the current page.
        /// </summary>
        [JsonProperty("subPages")]
        public List<DtWikiPage> SubPages { get; set; }

        /// <summary>
        /// REST url for this wiki page.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}