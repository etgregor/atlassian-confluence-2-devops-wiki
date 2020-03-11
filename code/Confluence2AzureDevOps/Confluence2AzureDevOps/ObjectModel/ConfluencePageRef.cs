using System.Collections.Generic;

namespace Confluence2AzureDevOps.ObjectModel
{
    /// <summary>
    /// References por create tree pages from index.html file.
    /// xpath selector //*[@id="content"]/div[2]/ul
    /// </summary>
    internal class ConfluencePageRef
    {
        public ConfluencePageRef()
        {
            SubPages = new List<ConfluencePageRef>();
        }
        
        public string HtmlTitle { get; set; }

        public string HtmlLocalFileName { get; set; }

        /// <summary>
        /// Name of file that result from conversion from Html to Markdown
        /// </summary>
        public string MarkdownLocalFilename { get; set; }
        
        /// <summary>
        /// Markdown local filename
        /// </summary>
        public string PageTitleAtAzureDevOps { get; set; }
        
        /// <summary>
        /// Property for 'wiki.path' on wiki Azure DevOps API
        /// <see cref="https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages/get%20page?view=azure-devops-rest-5.1#wikipage"/>
        /// </summary>
        public string PagePathAtAzureDevOps { get; set; }
        
        public List<ConfluencePageRef> SubPages { get; set; }
    }
}