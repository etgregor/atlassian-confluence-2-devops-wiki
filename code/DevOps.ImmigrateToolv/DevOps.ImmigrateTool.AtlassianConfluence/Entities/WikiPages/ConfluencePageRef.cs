using System.Collections.Generic;

namespace DevOps.ImmigrateTool.AtlassianConfluence.Entities.WikiPages
{
    /// <summary>
    /// References por create tree pages from index.html file.
    /// xpath selector //*[@id="content"]/div[2]/ul
    /// </summary>
    internal class ConfluencePageRef
    {
        public string Title { get; set; }

        public string File { get; set; }

        public List<ConfluencePageRef> SubPages { get; set; }
    }
}