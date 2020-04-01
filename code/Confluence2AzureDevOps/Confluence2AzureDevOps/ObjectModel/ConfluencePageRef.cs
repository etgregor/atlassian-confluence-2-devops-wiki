using System.Collections.Generic;
using Confluence2AzureDevOps.Base.CustomExceptions;

namespace Confluence2AzureDevOps.ObjectModel
{
    /// <summary>
    /// References por create tree pages from index.html file.
    /// xpath selector //*[@id="content"]/div[2]/ul
    /// </summary>
    public class ConfluencePageRef
    {
        private const string DOC_AZURE_WIKI =
            "https://docs.microsoft.com/en-us/azure/devops/project/wiki/wiki-file-structure?view=azure-devops";
        
        /// <summary>
        /// Max length for path at azure dev ops, <see cref="https://docs.microsoft.com/en-us/azure/devops/project/wiki/wiki-file-structure?view=azure-devops"/> 
        /// </summary>
        private const int AZURE_DEV_OPS_MAX_PATH_LENGTH = 235;
        
        public ConfluencePageRef()
        {
            SubPages = new List<ConfluencePageRef>();
        }

        public ConfluencePageRef(string caption, string localFilePath)
        {
            HtmlTitle = caption;
            HtmlLocalFileName = localFilePath;
            SubPages = new List<ConfluencePageRef>();
        }
        
        public string HtmlTitle { get; private set; }
        
        /// <summary>
        /// Name of html file in local storage
        /// </summary>
        public string HtmlLocalFileName { get; }

        // // /// <summary>
        // // /// I's the name of page at azure wiki ( r
        // // /// </summary>
        // public string MarkDownTitle { get; set; }
        
        /// <summary>
        /// Name of file that result from conversion from Html to Markdown
        /// </summary>
        public string MarkdownLocalFilename { get; private set; }
        
        /// <summary>
        /// Markdown local filename
        /// </summary>
        public string PageTitleAtAzureDevOps { get; private set; }
        
        /// <summary>
        /// Property for 'wiki.path' on wiki Azure DevOps API
        /// <see cref="https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages/get%20page?view=azure-devops-rest-5.1#wikipage"/>
        /// </summary>
        public string PagePathAtAzureDevOps { get; private set; }

        public bool? MigrationComplete { get; private set; }
        
        public string MigrationMessage { get; private set; }
        
        public void SetAzurePageInfo(string markdonFileName, string titleAtAzureWiki, string patAtAzureWiki)
        {
            MarkdownLocalFilename = markdonFileName;
            PageTitleAtAzureDevOps = titleAtAzureWiki;
            PagePathAtAzureDevOps = patAtAzureWiki;
            
            if (PagePathAtAzureDevOps.Length > AZURE_DEV_OPS_MAX_PATH_LENGTH)
            {
                //NotifyProcess($"WARN: Too long path ({AZURE_DEV_OPS_MAX_PATH_LENGTH}): {wikiPageInfo.PagePathAtAzureDevOps}");
                throw new GenericC2AException(
                    $"Result path '{PagePathAtAzureDevOps}' is too long, azure wiki allow path max '{AZURE_DEV_OPS_MAX_PATH_LENGTH}' size path. See: {DOC_AZURE_WIKI}");
            }
        }

        public void SetMigrationSuccess(string message)
        {
            this.MigrationComplete = true;
            this.MigrationMessage = message;
        }
        
        public void SetMigrationFail(string message)
        {
            this.MigrationComplete = false;
            this.MigrationMessage = message;
        }

        public List<ConfluencePageRef> SubPages { get; private set; }
    }
}