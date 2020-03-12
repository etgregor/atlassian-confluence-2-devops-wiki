using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// <summary>
        /// Folder contains wiki md result
        /// </summary>
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
 
        public BmlProcessNotifier ProcessNotifier { get; set; }
        
        private ConfluencePageRef _wikiMenu;

        private readonly Dictionary<string, List<LinkReference>> _linkReferences;
        
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

            _linkReferences = new Dictionary<string, List<LinkReference>>();
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
            
            _wikiMenu = ReadConfluenceIndexOfPages(selectorOfIndexControl);

            WriteJsonObject("_InitialMigrationTreeInfo.json", _wikiMenu);
            
            ExtractSiteReferences(_wikiMenu);

            //WriteJsonObject("_MigrationTreeInfo.json", _wikiMenu);
            
            WriteJsonObject("_LinkReferences.json", _linkReferences);
            
            return _wikiMenu;
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

            if (!HtmlUtils.TryReadDocumentAsHtml(_indexFileFullPath, out var htmlDoc, out var readHtmlErrors))
            {
                throw new GenericC2AException($"Can't get the confluence page index from: {readHtmlErrors}");
            }
            
            HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode(ulElementSelector);

            if (bodyNode == null)
            {   
                throw new GenericC2AException($"Cant get main menu element, xpath: {ulElementSelector}");
            }

            List<ConfluencePageRef> nodes = GetPageInfoFromHtmlLinkElement(bodyNode.ChildNodes);
                
            wikiMainPage = nodes.FirstOrDefault();
            
            if (wikiMainPage == null)
            {
                throw new GenericC2AException($"Can't get the confluence page index from: {_indexFileFullPath}");
            }

            return wikiMainPage;
        }

        private void ExtractSiteReferences(ConfluencePageRef wikiPageInfo)
        {
            ExtractLinkReferences(wikiPageInfo);
            
            foreach (ConfluencePageRef subPage in wikiPageInfo.SubPages)
            {
                ExtractSiteReferences(subPage);
            }
        }
        
        private void ExtractLinkReferences(ConfluencePageRef confluencePageRef)
        {
            var linksOnPage = new List<LinkReference>();
            
            var htmlSourceFolder = IoUtils.GetPathIfFileExists(_htmlSourceFolder, confluencePageRef.HtmlLocalFileName);

            if (!string.IsNullOrEmpty(htmlSourceFolder))
            {
                if (HtmlUtils.TryReadDocumentAsHtml(htmlSourceFolder, out HtmlDocument file, out string errorr))
                {    
                    linksOnPage = ReadLinklsElement(file.DocumentNode.ChildNodes);
                }
                else
                {
                    NotifyProcess($"WARN: Can't read file as HTML. {confluencePageRef.HtmlLocalFileName}");
                }
            }
            else
            {
                NotifyProcess($"WARN: Can't locate file {confluencePageRef.HtmlLocalFileName}");
            }

            _linkReferences.Add(confluencePageRef.HtmlLocalFileName, linksOnPage);
        }

        private List<LinkReference> ReadLinklsElement(HtmlNodeCollection nodes)
        {
            var result = new List<LinkReference>();

            foreach (HtmlNode child in nodes)
            {
                if (child.Name == "a")
                {
                    LinkReference linkRef = HtmlUtils.GetNodeInfo(child);

                    if (linkRef != null)
                    {
                        result.Add(linkRef);    
                    }
                }
                else if (child.HasChildNodes)
                {
                    var childrenNodes = ReadLinklsElement(child.ChildNodes);
                    result.AddRange(childrenNodes);
                }
            }

            return result;
        }
        
        private void ConvertHtml2Markdown2(string nodePath, ConfluencePageRef wikiPageInfo)
        {
            string markdownFileName = ConvertHtml2Markdown2(wikiPageInfo);

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
                ConvertHtml2Markdown2(nodeSubPath, subPage);
            }
        }
        
        /// <summary>
        /// Convert file format from 'Html' to 'MD'
        /// </summary>
        /// <param name="confluencePageRef">Html origin page info.</param>
        /// <returns>Return path of file Markdown format, that result from conversion Html > MD</returns>
        private string ConvertHtml2Markdown2(ConfluencePageRef confluencePageRef)
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

        private List<ConfluencePageRef> GetPageInfoFromHtmlLinkElement(HtmlNodeCollection nodes)
        {
            var result = new List<ConfluencePageRef>();

            ConfluencePageRef newNode = null;
            
            foreach (HtmlNode child in nodes)
            {
                if (child.Name == "a")
                {
                    LinkReference linkRef = HtmlUtils.GetNodeInfo(child);

                    if (linkRef != null)
                    {
                        if (string.IsNullOrEmpty(linkRef.LinkCaption))
                        {
                            // TODO: Change link caption
                            linkRef.LinkCaption = Guid.NewGuid().ToString();
                        }
                        
                        newNode = new ConfluencePageRef()
                        {
                            HtmlTitle = linkRef.LinkCaption,
                            HtmlLocalFileName = linkRef.LinkHref
                        };
                    }
                    
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

        private void WriteJsonObject<T>(string fileName, T value)
        {
            string jsonFile = Path.Combine(_workingDir, fileName);
            
            IoUtils.SaveObjectToFile(jsonFile, value);

            System.Diagnostics.Trace.TraceInformation($"Write file {jsonFile}");
        }
        private void NotifyProcess(string message)
        {
            ProcessNotifier?.Invoke(message);
        }
    }
}