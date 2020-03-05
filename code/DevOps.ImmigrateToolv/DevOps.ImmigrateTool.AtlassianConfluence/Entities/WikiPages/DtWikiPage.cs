using System.Collections.Generic;

namespace DevOps.ImmigrateTool.AtlassianConfluence.Entities.WikiPages
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
        public string content { get; set; }

        /// <summary>
        /// Path of the git item corresponding to the wiki page stored in the backing Git repository.
        /// </summary>
        public string gitItemPath { get; set; }

        /// <summary>
        /// When present, permanent identifier for the wiki page
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// True if a page is non-conforming, i.e. 1) if the name doesn't match page naming standards. 2) if the page does not have a valid entry in the appropriate order file.
        /// </summary>
        public bool isNonConformant { get; set; }

        /// <summary>
        /// True if this page has subpages under its path.
        /// </summary>
        public bool isParentPage { get; set; }

        /// <summary>
        /// Order of the wiki page, relative to other pages in the same hierarchy level.
        /// </summary>
        public int order { get; set; }

        /// <summary>
        /// Path of the wiki page.
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// Remote web url to the wiki page.
        /// </summary>
        public string remoteUrl { get; set; }

        /// <summary>
        /// List of subpages of the current page.
        /// </summary>
        public List<DtWikiPage> subPages { get; set; }

        /// <summary>
        /// REST url for this wiki page.
        /// </summary>
        public string url { get; set; }
    }
}