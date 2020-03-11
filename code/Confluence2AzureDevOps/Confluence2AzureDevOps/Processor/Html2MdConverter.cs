using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Confluence2AzureDevOps.Base.CustomExceptions;
using Confluence2AzureDevOps.ObjectModel;
using Confluence2AzureDevOps.Utils;
using HtmlAgilityPack;

namespace Confluence2AzureDevOps.Processor
{
    /// <summary>
    /// Convert Html file to Markdown file
    /// </summary>
    public class Html2MdConverter
    {
        private readonly string _htmlSourceFolder;
        private readonly string _mdDestinationFolder;

        private string _indexFileFullPath;
 
        /// <summary>
        /// Json file with Markdown wiki structure.
        /// </summary>
        private const string CONFLUENCE_WIKI_INDEX_JSON_MARKDOWN_INFO_FILE = "_MigrationTreeInfo.json";

        /// <summary>
        /// Init converter
        /// </summary>
        /// <param name="htmlSourceFolder">Folder with Html pages that exported from Confluence Cloud</param>
        /// <param name="mdDestinationFolder">Folder</param>
        public Html2MdConverter(string htmlSourceFolder, string mdDestinationFolder)
        {
            _htmlSourceFolder = htmlSourceFolder;
            _mdDestinationFolder = mdDestinationFolder;

            Guard.PreventStringEmpty("htmlSourceFolder", htmlSourceFolder);
            Guard.PreventStringEmpty("mdDestinationFolder", mdDestinationFolder);
        }
        
        /// <summary>
        /// Init process migration bases on <see cref="confluenceIndexFile"/>, <see cref="selectorOfIndexControl"/>,
        /// looking for series: ul>a>ul>, and take "a" html element value and 'href' attribute 
        /// </summary>
        /// <param name="confluenceIndexFile">File that contain the index of site of confluence exported wiki site</param>
        /// <param name="selectorOfIndexControl">xpath selector of UL menu element at index.html file</param>
        /// <exception cref="GenericC2AException">When something is wrong</exception>
        public void StartConvertion(string confluenceIndexFile = "index.html", string selectorOfIndexControl = "//*[@id='content']/div[2]/ul")
        {
            ValidateInitialInput(confluenceIndexFile);
            
            ConfluencePageRef wikiMenu = ReadConfluenceIndexOfPages(selectorOfIndexControl);

            string nodePath = string.Empty;
            
            ConvertHtml2Markdown(nodePath, wikiMenu);
            
            WriteSampleJsonMenu(CONFLUENCE_WIKI_INDEX_JSON_MARKDOWN_INFO_FILE, wikiMenu);
        }

        private void ValidateInitialInput(string confluenceIndexFile)
        {
            Guard.PreventDirectoryNotExistt(_htmlSourceFolder);

            IoUtils.CreateFolderPath(_mdDestinationFolder);
            
            _indexFileFullPath = IoUtils.GetPathIfFileExists(_htmlSourceFolder, confluenceIndexFile);
            
            if (string.IsNullOrEmpty(_indexFileFullPath))
            {
                throw new GenericC2AException(
                    $"{confluenceIndexFile} is required and not exists at {_htmlSourceFolder}");
            }
        }
        
        private ConfluencePageRef ReadConfluenceIndexOfPages(string ulElementSelector)
        {
            ConfluencePageRef wikiMainPage = null;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.Load(_indexFileFullPath);

            // ParseErrors is an ArrayList containing any errors from the Load statement
            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
            {
                var errors = new StringBuilder();

                foreach (HtmlParseError error in htmlDoc.ParseErrors)
                {
                    errors.AppendLine(error.Reason);
                }

                throw new GenericC2AException(errors.ToString());
            }

            if (htmlDoc.DocumentNode != null)
            {
                HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode(ulElementSelector);

                if (bodyNode == null)
                {   
                    throw new GenericC2AException($"Cant get main menu element, xpath: {ulElementSelector}");
                }
                
                GetPageInfoFromHtmlLinkElement(ref wikiMainPage, bodyNode);
            }
            
            if (wikiMainPage == null)
            {
                throw new GenericC2AException($"Can't get the confluence page index from: {_indexFileFullPath}");
            }

            return wikiMainPage;
        }

        private void WriteSampleJsonMenu(string fileName, ConfluencePageRef wikiMenu)
        {
            string jsonFile = Path.Combine(_mdDestinationFolder, fileName);
            
            IoUtils.SaveObjectToFile(jsonFile, wikiMenu);

            System.Diagnostics.Trace.TraceInformation($"Write file {jsonFile}");
        }
        
        private void ConvertHtml2Markdown(string nodePath, ConfluencePageRef siteIndex)
        {
            System.Diagnostics.Trace.TraceInformation($"Start conversion file: {siteIndex.HtmlLocalFileName}");
            
            string markdownFileName = ConvertHtml2Markdown(siteIndex.HtmlLocalFileName);

            if (!string.IsNullOrEmpty(markdownFileName))
            {
                System.Diagnostics.Trace.TraceInformation($"New file created: {markdownFileName}");
                siteIndex.MarkdownLocalFilename = markdownFileName;
                siteIndex.PageTitleAtAzureDevOps = siteIndex.HtmlTitle;
                siteIndex.PagePathAtAzureDevOps = $"{nodePath}/{siteIndex.HtmlTitle}";
            }
            else
            {
                System.Diagnostics.Trace.TraceInformation($"WARN: Failed convertion file: {siteIndex.HtmlLocalFileName}");
            }

            string nodeSubPath = $"{nodePath}/{siteIndex.PageTitleAtAzureDevOps}";
            
            foreach (ConfluencePageRef subPage in siteIndex.SubPages)
            {
                ConvertHtml2Markdown(nodeSubPath, subPage);
            }
        }

        /// <summary>
        /// Convert file format from 'Html' to 'MD'
        /// </summary>
        /// <param name="htmlFileName">Html file original for apply conversion.</param>
        /// <returns>Return path of file Markdown format, that result from conversion Html > MD</returns>
        private string ConvertHtml2Markdown(string htmlFileName)
        {
            string localFileMarkdownName = string.Empty;
            
            var filePath = IoUtils.GetPathIfFileExists(_htmlSourceFolder, htmlFileName);

            if (!string.IsNullOrEmpty(filePath))
            {
                string htmlFileContent = IoUtils.ReadFileContent(filePath);
                
                var converter = new ReverseMarkdown.Converter();

                string result = converter.Convert(htmlFileContent);

                localFileMarkdownName = string.Format("{0}.md", Path.GetFileNameWithoutExtension(htmlFileName));
                
                string fileDestinyFullPath = System.IO.Path.Combine(_mdDestinationFolder, localFileMarkdownName);
                
                IoUtils.SaveFile(fileDestinyFullPath, result);
            }

            return localFileMarkdownName;
        }
        
        /// <summary>
        /// Get page info tree, href=File route, valule = title
        /// </summary>
        /// <param name="confluencePageRef"></param>
        /// <param name="bodyNode"></param>
        private void GetPageInfoFromHtmlLinkElement(ref ConfluencePageRef confluencePageRef, HtmlNode bodyNode )
        {
            ConfluencePageRef lastPage = null;
                
            foreach (HtmlNode child in bodyNode.ChildNodes.Descendants())
            {
                if (child.Name == "a")
                {
                    try
                    {
                        string fileName = child.GetAttributeValue("href", string.Empty);

                        string pageTitle = child.InnerText;

                        if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(pageTitle))
                        {
                            pageTitle = CleanupFileTitle(pageTitle);
                            
                            if (confluencePageRef == null)
                            {
                                confluencePageRef = new ConfluencePageRef();
                                
                                confluencePageRef.HtmlTitle = pageTitle;
                                confluencePageRef.HtmlLocalFileName = fileName;

                                lastPage = confluencePageRef;
                            }
                            else
                            {
                                lastPage = new ConfluencePageRef();
                                lastPage.HtmlTitle = pageTitle;
                                lastPage.HtmlLocalFileName = fileName;

                                confluencePageRef.SubPages.Add(lastPage);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.TraceError("Error when try to get page info: {0}", e);
                    }
                }
                else if (child.HasChildNodes)
                {
                    GetPageInfoFromHtmlLinkElement(ref lastPage, child);
                }
            }
        }

        /// <summary>
        /// Remove many empty spaces, and invalid chars for filename
        /// <see cref="https://docs.microsoft.com/en-us/azure/devops/project/wiki/wiki-file-structure?view=azure-devops"/>
        /// </summary>
        /// <param name="title">original </param>
        /// <returns>Clean text</returns>
        private string CleanupFileTitle(string title)
        {
            title = title.Replace("\n", " ");
            title = title.Replace("/", "-");
            
            title = Regex.Replace(title, @"\s+", " ");
            
            return title;
        }
    }
}