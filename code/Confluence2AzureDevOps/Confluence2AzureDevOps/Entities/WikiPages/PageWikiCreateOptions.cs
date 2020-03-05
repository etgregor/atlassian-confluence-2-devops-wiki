namespace Confluence2AzureDevOps.Entities.WikiPages
{
    public class PageWikiCreateOptions
    {
        /// <summary>
        /// Wiki page path, include page name. i.g.: page1 
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Comment to be associated with the page operation.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Content of the wiki page. Syntax guidance for basic Markdown usage <see cref="https://docs.microsoft.com/en-us/azure/devops/project/wiki/markdown-guidance?view=azure-devops">here</see>
        /// </summary>
        public string Content { get; set; }
    }
}