using System.Collections.Generic;

namespace Confluence2AzureDevOps.Entities.WikiPages
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

        public string MarkdownTitle { get; set; }
        
        public string MarkdownLocalFileName { get; set; }
        
        public List<ConfluencePageRef> SubPages { get; set; }
    }
}