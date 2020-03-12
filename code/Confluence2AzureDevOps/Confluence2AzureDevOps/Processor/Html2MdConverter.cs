using System;
using System.Collections.Generic;
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
        private const string WIKI_MD_FILES = "WikiMd";
        
        /// <summary>
        /// Folder with Html pages that exported from Confluence Cloud
        /// </summary>
        private readonly string _htmlSourceFolder;
        
        /// <summary>
        /// Local working directory. It will create:
        /// - "MD/" folder for conversion result
        /// - "MD/.attachments/" folder for attachments files
        /// </summary>
        private readonly string _workingDir;

        /// <summary>
        /// 
        /// </summary>
        private readonly string _wikiOutputResult;
        
        private string _indexFileFullPath;
 
        /// <summary>
        /// Json file with Markdown wiki structure.
        /// </summary>
        private const string CONFLUENCE_WIKI_INDEX_JSON_MARKDOWN_INFO_FILE = "_MigrationTreeInfo.json";

        public BmlProcessNotifier ProcessNotifier { get; set; }
            
        /// <summary>
        /// Init converter
        /// </summary>
        /// <param name="htmlSourceFolder">Folder with Html pages that exported from Confluence Cloud</param>
        /// <param name="mdWorkingDirectory">
        /// Local working directory. It will create:
        /// - "WikiMd/" folder for conversion result
        /// - "WikiMd/.attachments/" folder for attachments files
        /// </param>
        public Html2MdConverter(string htmlSourceFolder, string mdWorkingDirectory)
        {
            Guard.PreventStringEmpty("htmlSourceFolder", htmlSourceFolder);
            Guard.PreventStringEmpty("mdDestinationFolder", mdWorkingDirectory);
            
            _htmlSourceFolder = htmlSourceFolder;
            _workingDir = mdWorkingDirectory;

            _wikiOutputResult = Path.Combine(mdWorkingDirectory, WIKI_MD_FILES);
        }
        
        /// <summary>
        /// Init process migration bases on <see cref="confluenceIndexFile"/>, <see cref="selectorOfIndexControl"/>,
        /// looking for series: ul>a>ul>, and take "a" html element value and 'href' attribute 
        /// </summary>
        /// <param name="confluenceIndexFile">File that contain the index of site of confluence exported wiki site</param>
        /// <param name="selectorOfIndexControl">xpath selector of UL menu element at index.html file</param>
        /// <exception cref="GenericC2AException">When something is wrong</exception>
        public ConfluencePageRef ConvertHtmlToMdFiles(string confluenceIndexFile = "index.html", string selectorOfIndexControl = "//*[@id='content']/div[2]/ul")
        {
            ValidateInitialInput(confluenceIndexFile);
            
            CreateOutputDirs();
            
            ConfluencePageRef wikiMenu = ReadConfluenceIndexOfPages(selectorOfIndexControl);

            string nodePath = string.Empty;
            
            WriteSampleJsonMenu("_InitialMigrationTreeInfo.json", wikiMenu);
            
            ConvertHtml2Markdown(nodePath, wikiMenu);

            WriteSampleJsonMenu(CONFLUENCE_WIKI_INDEX_JSON_MARKDOWN_INFO_FILE, wikiMenu);
            
            return wikiMenu;
        }

        private void ValidateInitialInput(string confluenceIndexFile)
        {
            Guard.PreventDirectoryNotExistt(_htmlSourceFolder);
            
            _indexFileFullPath = IoUtils.GetPathIfFileExists(_htmlSourceFolder, confluenceIndexFile);
            
            if (string.IsNullOrEmpty(_indexFileFullPath))
            {
                throw new GenericC2AException(
                    $"{confluenceIndexFile} is required and not exists at {_htmlSourceFolder}");
            }
        }

        private void CreateOutputDirs()
        {
            IoUtils.CreateFolderPath(_wikiOutputResult);
        }
        
        private ConfluencePageRef ReadConfluenceIndexOfPages(string ulElementSelector)
        {
            ConfluencePageRef wikiMainPage = null;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.Load(_indexFileFullPath);

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

                List<ConfluencePageRef> nodes = GetPageInfoFromHtmlLinkElement(bodyNode.ChildNodes);
                
                wikiMainPage = nodes.FirstOrDefault();
            }
            
            if (wikiMainPage == null)
            {
                throw new GenericC2AException($"Can't get the confluence page index from: {_indexFileFullPath}");
            }

            return wikiMainPage;
        }

        private void ConvertHtml2Markdown(string nodePath, ConfluencePageRef wikiPageInfo)
        {
            string markdownFileName = ConvertHtml2Markdown(wikiPageInfo);

            if (!string.IsNullOrEmpty(markdownFileName))
            {
                wikiPageInfo.MarkdownLocalFilename = markdownFileName;
                wikiPageInfo.PageTitleAtAzureDevOps = wikiPageInfo.HtmlTitle;
                wikiPageInfo.PagePathAtAzureDevOps = $"{nodePath}/{wikiPageInfo.HtmlTitle}";

                NotifyProcess($"CONVERTED {wikiPageInfo.HtmlLocalFileName} => {markdownFileName}");
            }
            else
            {
                NotifyProcess($"WARN: Failed convertion file: {wikiPageInfo.HtmlLocalFileName}");
            }

            string nodeSubPath = $"{nodePath}/{wikiPageInfo.PageTitleAtAzureDevOps}";
            
            foreach (ConfluencePageRef subPage in wikiPageInfo.SubPages)
            {
                ConvertHtml2Markdown(nodeSubPath, subPage);
            }
        }

        /// <summary>
        /// Convert file format from 'Html' to 'MD'
        /// </summary>
        /// <param name="confluencePageRef">Html origin page info.</param>
        /// <returns>Return path of file Markdown format, that result from conversion Html > MD</returns>
        private string ConvertHtml2Markdown(ConfluencePageRef confluencePageRef)
        {
            string newMarkdownFilename = string.Empty;
            
            var htmlSourceFolder = IoUtils.GetPathIfFileExists(_htmlSourceFolder, confluencePageRef.HtmlLocalFileName);

            if (!string.IsNullOrEmpty(htmlSourceFolder))
            {
                string htmlFileContent = IoUtils.ReadFileContent(htmlSourceFolder);
                
                var converter = new ReverseMarkdown.Converter();

                string mdFileContent = converter.Convert(htmlFileContent);

                newMarkdownFilename =  $"{Path.GetFileNameWithoutExtension(confluencePageRef.HtmlTitle)}.md";
                
                string fileDestinyFullPath = Path.Combine(_wikiOutputResult, newMarkdownFilename);

                string fileExists = IoUtils.GetPathIfFileExists(fileDestinyFullPath, mdFileContent);
                
                if (string.IsNullOrEmpty(fileExists))
                {
                    IoUtils.SaveFile(fileDestinyFullPath, mdFileContent);
                }
                else
                {
                    ProcessNotifier($"WARN: File already exists {fileExists}");
                }
            }

            return newMarkdownFilename;
        }

        /// <summary>
        /// Get page info tree, href=File route, valule = title
        /// </summary>
        /// <param name="confluencePageRef"></param>
        /// <param name="bodyNode"></param>
//        private void GetPageInfoFromHtmlLinkElement(ref ConfluencePageRef parentNode, HtmlNode bodyNode)
//        {
//            ConfluencePageRef subNodeInfo = null;
//            
//            foreach (HtmlNode child in bodyNode.ChildNodes)
//            {
//                if (child.Name == "a")
//                {
//                    if (subNodeInfo == null)
//                    {
//                        subNodeInfo = GetNodeInfo(child);
//                    }
//                }
//                else if (child.HasChildNodes)
//                {
//                    GetPageInfoFromHtmlLinkElement(ref subNodeInfo, child);
//                }
//            }
//
//            if (subNodeInfo != null)
//            {
//                if (parentNode == null)
//                {
//                    parentNode = subNodeInfo;
//                }
//                else
//                {
//                    parentNode.SubPages.Add(subNodeInfo);
//                }
//            }
//        }

        private List<ConfluencePageRef> GetPageInfoFromHtmlLinkElement(HtmlNodeCollection nodes)
        {
            var result = new List<ConfluencePageRef>();

            ConfluencePageRef newNode = null;
            
            foreach (HtmlNode child in nodes)
            {
                if (child.Name == "a")
                {   
                    newNode = GetNodeInfo(child);
                    result.Add(newNode);
                }
                else if (child.HasChildNodes)
                {
                    var childrenNodes = GetPageInfoFromHtmlLinkElement(child.ChildNodes);
                    
                    if (newNode != null)
                    {
                        newNode.SubPages.AddRange(childrenNodes);
                    }
                    else
                    {
                        result.AddRange(childrenNodes);
                    }
                }
            }

            return result;
        }

        private ConfluencePageRef GetNodeInfo(HtmlNode htmlNode)
        {
            ConfluencePageRef pageInfo = null;
            
            try
            {
                string fileName = htmlNode.GetAttributeValue("href", string.Empty);

                string pageTitle = htmlNode.InnerText;

                if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(pageTitle))
                {
                    pageTitle = CleanupFileTitle(pageTitle);

                    pageInfo = new ConfluencePageRef()
                    {
                        HtmlTitle = pageTitle,
                        HtmlLocalFileName = fileName
                    };
                }
                else
                {
                    NotifyProcess($"Possible error empty <a> node");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError("ERROR (GetNodeInfo): {0}", e);
                        
                NotifyProcess($"ERROR: when try to get page info: {e.Message}");
            }

            return pageInfo;
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

        private void WriteSampleJsonMenu(string fileName, ConfluencePageRef wikiMenu)
        {
            string jsonFile = Path.Combine(_workingDir, fileName);
            
            IoUtils.SaveObjectToFile(jsonFile, wikiMenu);

            System.Diagnostics.Trace.TraceInformation($"Write file {jsonFile}");
        }
        private void NotifyProcess(string message)
        {
            ProcessNotifier?.Invoke(message);
        }
    }
}