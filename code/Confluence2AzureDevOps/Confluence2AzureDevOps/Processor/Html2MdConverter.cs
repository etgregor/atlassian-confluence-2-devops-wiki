using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Confluence2AzureDevOps.Base.CustomExceptions;
using Confluence2AzureDevOps.ObjectModel;
using Confluence2AzureDevOps.ObjectModel.HtmlElements;
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
        /// Original html files that have to transform
        /// </summary>
        private const string CAPTION_NOT_SET = "captionNotSet";
        
        /// <summary>
        /// Original html files that have to transform
        /// </summary>
        private const string WIKI_ORIGINAL_HTML_FILES = "1_OriginaHtml";
        
        /// <summary>
        /// Processed files ( remove invalid tags, replace custom tags )
        /// </summary>
        private const string WIKI_PROCESSED_HTML_FILES = "2_ProcessedHtml";
        
        /// <summary>
        /// Folder contains wiki md result
        /// </summary>
        private const string WIKI_RESULT_MD_FILES = "3_ResultWikiMd";
        
        /// <summary>
        /// Folder contains wiki md result
        /// </summary>
        private const string WIKI_RESULT_ATTACHMENT_FILES = ".attachments";
        
        private const int AZURE_DEVOPS_MAX_PATH_LENGTH =235;
        
        /// <summary>
        /// Folder with Html pages that exported from Confluence Cloud
        /// </summary>
        private readonly string _htmlSourceFolder;
        
        private readonly string _rootWorkingDir;

        private readonly string _originalHtmlDir;
        
        private readonly string _processedHtmlDir;
        
        private readonly string _resultDir;
        
        private readonly string _resultAttachmentsDir;
        
        private string _indexFileFullPath;

        private int _captionNotSetCount;
 
        public BmlProcessNotifier ProcessNotifier { get; set; }
        
        private ConfluencePageRef _wikiMenu;

        private readonly Dictionary<string, List<LinkElementInfo>> _linkReferences;
        
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
            
            _rootWorkingDir = mdWorkingDirectory;

            _originalHtmlDir = Path.Combine(mdWorkingDirectory, WIKI_ORIGINAL_HTML_FILES);

            _processedHtmlDir = Path.Combine(mdWorkingDirectory, WIKI_PROCESSED_HTML_FILES);
            
            _resultDir = Path.Combine(mdWorkingDirectory, WIKI_RESULT_MD_FILES);

            _resultAttachmentsDir = Path.Combine(mdWorkingDirectory, WIKI_RESULT_MD_FILES, WIKI_RESULT_ATTACHMENT_FILES);
                
            _linkReferences = new Dictionary<string, List<LinkElementInfo>>();

            _captionNotSetCount = 0;
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

            BuildRoutePathTree(string.Empty, _wikiMenu);
            
            WriteJsonObject("_InitialMigrationTreeInfo.json", _wikiMenu);
            
            PreprocessHtmlFile(_wikiMenu);

            //WriteJsonObject("_MigrationTreeInfo.json", _wikiMenu);
            
            WriteJsonObject("_LinkReferences.json", _linkReferences);
            
            return _wikiMenu;
        }

        #region - Initial -

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
                    IoUtils.CreateFolderPath(_originalHtmlDir);
                    IoUtils.CreateFolderPath(_processedHtmlDir);
                    IoUtils.CreateFolderPath(_resultDir);
                    IoUtils.CreateFolderPath(_resultAttachmentsDir);
                }


        #endregion
       
        #region - ExtractInitialIndex -

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

            wikiMainPage.HtmlTitle = "Home";
            
            return wikiMainPage;
        }
        
        private void BuildRoutePathTree(string nodePath, ConfluencePageRef wikiPageInfo)
        {
            string fileTitleDevOps = GetPageTitleAtForAzureDevOps(wikiPageInfo);

            if (!string.IsNullOrEmpty(fileTitleDevOps))
            {
                wikiPageInfo.PageTitleAtAzureDevOps = fileTitleDevOps;
                wikiPageInfo.MarkdownLocalFilename = $"{fileTitleDevOps}.md";
                
                wikiPageInfo.PagePathAtAzureDevOps = $"{nodePath}/{wikiPageInfo.HtmlTitle}";

                if (wikiPageInfo.PagePathAtAzureDevOps.Length > AZURE_DEVOPS_MAX_PATH_LENGTH)
                {
                    NotifyProcess($"WARN: Too long path ({AZURE_DEVOPS_MAX_PATH_LENGTH}): {wikiPageInfo.PagePathAtAzureDevOps}");
                }
            }
            else
            {
                NotifyProcess($"WARN: file not migrate: {wikiPageInfo.HtmlLocalFileName}");
            }

            string nodeSubPath = $"{nodePath}/{wikiPageInfo.PageTitleAtAzureDevOps}";
            
            foreach (ConfluencePageRef subPage in wikiPageInfo.SubPages)
            {
                BuildRoutePathTree(nodeSubPath, subPage);
            }
        }
        
        private string GetPageTitleAtForAzureDevOps(ConfluencePageRef confluencePageRef)
        {
            string pageTitle = string.Empty;
            
            string htmlSourceFolder = IoUtils.GetPathIfFileExists(_htmlSourceFolder, confluencePageRef.HtmlLocalFileName);

            if (!string.IsNullOrEmpty(htmlSourceFolder))
            {
                pageTitle = HtmlUtils.ConvertToValidWikiFileName(confluencePageRef.HtmlTitle);
            }

            return pageTitle;
        }

        #endregion

        #region - Preprocesing file-

        private void PreprocessHtmlFile(ConfluencePageRef wikiPageInfo)
        {
            PrepareHtmlFile(wikiPageInfo);

            foreach (ConfluencePageRef subPage in wikiPageInfo.SubPages)
            {
                PreprocessHtmlFile(subPage);
            }
        }

        private void PrepareHtmlFile(ConfluencePageRef confluencePageRef)
        {
            HtmlDocument htmlPage = ReadOriginalFileAsHtml(confluencePageRef.HtmlLocalFileName);
    
            if (htmlPage != null)
            {
                RemoveInvalidNodes(confluencePageRef.HtmlLocalFileName,  htmlPage.DocumentNode.ChildNodes);
                
                List<LinkElementInfo> linksOnPage = GetLinksElement(htmlPage.DocumentNode.ChildNodes);
                
                _linkReferences.Add(confluencePageRef.HtmlLocalFileName, linksOnPage);

                string preprocessedHtml = Path.Combine(_processedHtmlDir, confluencePageRef.HtmlLocalFileName);
                
                htmlPage.Save(preprocessedHtml);
            }
        }

        private HtmlDocument ReadOriginalFileAsHtml(string htmlLocalFileName)
        {
            HtmlDocument file = null;
            
            var htmlSourceFolder = IoUtils.GetPathIfFileExists(_htmlSourceFolder, htmlLocalFileName);

            if (!string.IsNullOrEmpty(htmlSourceFolder))
            {
               // - backup file 
                var backupFile = Path.Combine(_originalHtmlDir, htmlLocalFileName);

                File.Copy(htmlSourceFolder, backupFile);
                // - backup file
                
                if (HtmlUtils.TryReadDocumentAsHtml(htmlSourceFolder, out file, out string readingErrors))
                {
                    if (!string.IsNullOrEmpty(readingErrors))
                    {
                        NotifyProcess($"WARN: Read file with errors. {htmlLocalFileName}: {readingErrors}");
                    }
                }
                else
                {
                    NotifyProcess($"WARN: Can't read file as HTML. {htmlLocalFileName}: {readingErrors}");
                }
            }
            else
            {
                NotifyProcess($"WARN: file not exists. {htmlLocalFileName}");
            }

            return file;
        }

        private List<LinkElementInfo> GetLinksElement(HtmlNodeCollection nodes)
        {
            var result = new List<LinkElementInfo>();

            foreach (HtmlNode child in nodes)
            {
                if (child.Name == "a")
                {
                    LinkElementInfo linkRef = HtmlUtils.GetLinkInfo(child);

                    if (linkRef != null)
                    {
                        result.Add(linkRef);    
                    }
                }
                else if (child.Name == "img")
                {
                    LinkElementInfo linkRef = HtmlUtils.GetImgInfo(child);
                    
                    if (linkRef != null)
                    {
                        result.Add(linkRef);    
                    }
                }
                else if (child.HasChildNodes)
                {
                    var childrenNodes = GetLinksElement(child.ChildNodes);
                    result.AddRange(childrenNodes);
                }
            }

            return result;
        }

        private void RemoveInvalidNodes(string fileName, HtmlNodeCollection nodes)
        {
            var nodesToRemove = new List<HtmlNode>();

            foreach (HtmlNode child in nodes)
            {
                if (child.Name == "script")
                {
                    nodesToRemove.Add(child);
                }
                else if (child.Name == "head")
                {
                    nodesToRemove.Add(child);
                }
                else if (child.Name == "div" && HtmlUtils.TryGetCodeSnipped(child, out CodeSectionInfo codeSectionInfo))
                {
                    NotifyProcess($"Code section fount: {fileName}");
                    NotifyProcess(codeSectionInfo.ToString());

                    child.InnerHtml = codeSectionInfo.ToString();
                }
                else if(!child.HasChildNodes)
                {
                    child.InnerHtml = child.InnerHtml.Trim();
                }
                else if (child.HasChildNodes)
                {
                    RemoveInvalidNodes(fileName, child.ChildNodes);
                }
            }

            foreach (HtmlNode htmlNode in nodesToRemove)
            {
                nodes.Remove(htmlNode);
            }
        }
        
        #endregion
        
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
                
                string fileDestinyFullPath = Path.Combine(_resultDir, newMarkdownFilename);

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
                    LinkElementInfo linkRef = HtmlUtils.GetLinkInfo(child);

                    if (linkRef != null)
                    {
                        if (string.IsNullOrEmpty(linkRef.LinkCaption))
                        {
                            linkRef.LinkCaption = $"{CAPTION_NOT_SET}_{_captionNotSetCount}";
                            _captionNotSetCount++;
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
                    List<ConfluencePageRef> childrenNodes = GetPageInfoFromHtmlLinkElement(child.ChildNodes);
                    
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
            string jsonFile = Path.Combine(_rootWorkingDir, fileName);
            
            IoUtils.SaveObjectToFile(jsonFile, value);

            System.Diagnostics.Trace.TraceInformation($"Write file {jsonFile}");
        }
        
        private void NotifyProcess(string message)
        {
            ProcessNotifier?.Invoke(message);
        }
    }
}