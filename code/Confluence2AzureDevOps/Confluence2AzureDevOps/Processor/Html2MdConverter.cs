using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

        /// <summary>
        /// Folder with Html pages that exported from Confluence Cloud
        /// </summary>
        private readonly string _htmlSourceFolder;
        
        private readonly string _rootWorkingDir;

        private readonly string _backupHtmlDir;
        
        private readonly string _processedHtmlDir;
        
        private readonly string _resultDir;
        
        private readonly string _resultAttachmentsDir;
        
        private string _indexFileFullPath;

        private int _captionNotSetCount;
        
        private int _internalRefCountNumber;
 
        public BmlProcessNotifier ProcessNotifier { get; set; }

        public Dictionary<string, List<LinkElementInfo>> AttachmentsFiles
        {
            get { return _linkReferences; }
        }

        private ConfluencePageRef _wikiMenu;

        private readonly Dictionary<string, List<LinkElementInfo>> _linkReferences;

        private Dictionary<string, string> _replazableTitles;
        
        /// <summary>
        /// Init converter
        /// </summary>
        /// <param name="htmlSourceFolder">Folder with Html pages that exported from Confluence Cloud</param>
        /// <param name="mdWorkingDirectory">
        /// Local working directory. It will create:
        /// - "WikiMd/" folder for conversion result
        /// - "WikiMd/.attachments/" folder for attachments files
        /// </param>
        /// <param name="replazableTitles">it can use for replace long titles</param>
        public Html2MdConverter(string htmlSourceFolder, string mdWorkingDirectory, Dictionary<string, string> replazableTitles = null)
        {
            Guard.PreventStringEmpty("htmlSourceFolder", htmlSourceFolder);
            Guard.PreventStringEmpty("mdDestinationFolder", mdWorkingDirectory);
            
            _htmlSourceFolder = htmlSourceFolder;
            
            _rootWorkingDir = mdWorkingDirectory;

            _replazableTitles = replazableTitles;
            
            _backupHtmlDir = Path.Combine(mdWorkingDirectory, WIKI_ORIGINAL_HTML_FILES);

            _processedHtmlDir = Path.Combine(mdWorkingDirectory, WIKI_PROCESSED_HTML_FILES);
            
            _resultDir = Path.Combine(mdWorkingDirectory, WIKI_RESULT_MD_FILES);

            _resultAttachmentsDir = Path.Combine(mdWorkingDirectory, WIKI_RESULT_MD_FILES, WIKI_RESULT_ATTACHMENT_FILES);
                
            _linkReferences = new Dictionary<string, List<LinkElementInfo>>();

            _captionNotSetCount = 1;
            
            _internalRefCountNumber = 1;

            if (_replazableTitles == null)
            {
                _replazableTitles = new Dictionary<string, string>();
            }
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
            Guard.PreventDirectoryNotExistt(_htmlSourceFolder);

            _indexFileFullPath = GetFullPathIndexPage(confluenceIndexFile);
            
            CreateOutputDirs();
            
            _wikiMenu = ReadConfluenceMapSite(selectorOfIndexControl);

            BuildRoutePathTree(string.Empty, _wikiMenu);
            
            WriteJsonObject("_InitialMigrationTreeInfo.json", _wikiMenu);
            
            ProcessHtmlFile(_wikiMenu);
            
            WriteJsonObject("_LinkReferences.json", _linkReferences);
            
            return _wikiMenu;
        }

        #region - Initial -

        private string GetFullPathIndexPage(string confluenceIndexFile)
        {
            string indexFileFullPath = IoUtils.GetPathIfFileExists(_htmlSourceFolder, confluenceIndexFile);

            if (string.IsNullOrEmpty(indexFileFullPath))
            {
                throw new GenericC2AException(
                    $"{confluenceIndexFile} is required and not exists at {_htmlSourceFolder}");
            }

            return indexFileFullPath;
        }

        private void CreateOutputDirs()
        {
            IoUtils.CreateFolderPath(_backupHtmlDir);
            IoUtils.CreateFolderPath(_processedHtmlDir);
            IoUtils.CreateFolderPath(_resultDir);
            IoUtils.CreateFolderPath(_resultAttachmentsDir);
        }


        #endregion
       
        #region - ExtractInitialIndex -

        private ConfluencePageRef ReadConfluenceMapSite(string ulElementSelector)
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

            string mainFile = Path.GetFileName(_indexFileFullPath);

            wikiMainPage = new ConfluencePageRef("Home", mainFile);
            wikiMainPage.SubPages.AddRange(nodes);

            if (wikiMainPage == null)
            {
                throw new GenericC2AException($"Can't get the confluence page index from: {_indexFileFullPath}");
            }

            return wikiMainPage;
        }

        private void BuildRoutePathTree(string nodePath, ConfluencePageRef wikiPageInfo)
        {
            string fileTitleDevOps = GetPageTitleAtForAzureDevOps(wikiPageInfo);

            if (!string.IsNullOrEmpty(fileTitleDevOps))
            {
                var htmlFileName = System.IO.Path.GetFileNameWithoutExtension(wikiPageInfo.HtmlLocalFileName);
             
                wikiPageInfo.SetAzurePageInfo($"{htmlFileName}.md", fileTitleDevOps, $"{nodePath}/{fileTitleDevOps}");
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
        
        private List<ConfluencePageRef> GetPageInfoFromHtmlLinkElement(HtmlNodeCollection nodes)
        {
            var result = new List<ConfluencePageRef>();

            ConfluencePageRef newNode = null;
            
            foreach (HtmlNode child in nodes)
            {
                if (child.Name == HtmlConstants.HTML_A)
                {
                    LinkElementInfo linkRef = HtmlUtils.GetLinkInfo(child);

                    if (linkRef != null)
                    {
                        if (string.IsNullOrEmpty(linkRef.Caption))
                        {
                            linkRef.Caption = $"{CAPTION_NOT_SET}_{_captionNotSetCount}";
                            _captionNotSetCount++;
                        }

                        if (_replazableTitles.TryGetValue(linkRef.Caption, out string newCaption))
                        {
                            linkRef.Caption = newCaption;
                        }

                        newNode = new ConfluencePageRef(linkRef.Caption, linkRef.OriginalRef);
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

        #endregion

        #region - Preprocesing file-

        public void ProcessHtmlFile(ConfluencePageRef confluencePageRef)
        {
            HtmlDocument htmlPage = ReadOriginalFileAsHtml(confluencePageRef.HtmlLocalFileName);

            if (htmlPage != null)
            {
                NotifyProcess($"========> Start processing: {confluencePageRef.HtmlLocalFileName}");
                
                ApplyCustomConversions(htmlPage.DocumentNode.ChildNodes);

                List<LinkElementInfo> linksOnPage = GetLinksElement(htmlPage.DocumentNode.ChildNodes);
  
                _linkReferences.Add(confluencePageRef.HtmlLocalFileName, linksOnPage);

                string preprocessedHtml = Path.Combine(_processedHtmlDir, confluencePageRef.HtmlLocalFileName);

                // processed html file
                htmlPage.Save(preprocessedHtml);

                var htmlContent = new StringBuilder();
                htmlContent.Append(htmlPage.DocumentNode.OuterHtml);

                htmlContent = ReplaceLinks(confluencePageRef, htmlContent);
                
                ConvertHtml2Markdown2(confluencePageRef, htmlContent.ToString());
            }
            
            foreach (ConfluencePageRef subPage in confluencePageRef.SubPages)
            {
                ProcessHtmlFile(subPage);
            }
        }

        private HtmlDocument ReadOriginalFileAsHtml(string htmlLocalFileName)
        {
            HtmlDocument file = null;
            
            var htmlSourceFolder = IoUtils.GetPathIfFileExists(_htmlSourceFolder, htmlLocalFileName);

            if (!string.IsNullOrEmpty(htmlSourceFolder))
            {
                // - backup file 
                // var backupFile = Path.Combine(_originalHtmlDir, htmlLocalFileName);
                //
                // if (File.Exists(backupFile))
                // {
                //     File.Delete(backupFile);
                // }
                //
                // File.Copy(htmlSourceFolder, backupFile);
                // - backup file
                
                if (HtmlUtils.TryReadDocumentAsHtml(htmlSourceFolder, out file, out string readingErrors))
                {
                    if (!string.IsNullOrEmpty(readingErrors))
                    {
                        NotifyProcess($"WARN: HTML errors: {readingErrors}");
                    }
                }
                else
                {
                    NotifyProcess($"WARN: Can't read file as HTML. {htmlLocalFileName}: {readingErrors}");
                }
            }
            else
            {
                NotifyProcess($"WARN: File exists.");
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

        private void ApplyCustomConversions(HtmlNodeCollection nodes)
        {
            var elementsToRemove = new List<HtmlNode>();

            foreach (HtmlNode nodeToAnalyze in nodes)
            {
                if (string.Equals(nodeToAnalyze.Name, HtmlConstants.HTML_SCRIPT)  
                    || string.Equals(nodeToAnalyze.Name, HtmlConstants.HTML_HEAD))
                {
                    // script, head
                    elementsToRemove.Add(nodeToAnalyze);
                }
                else if (string.Equals(nodeToAnalyze.Name, HtmlConstants.HTML_DIV) 
                         && HtmlUtils.ContainsTableElement(nodeToAnalyze, out HtmlNode tableNode))
                {
                    // table
                    var cleaner = new HtmlTableCleaner(tableNode, _internalRefCountNumber);
                    nodeToAnalyze.InnerHtml = cleaner.GetTableDefinition();

                    _internalRefCountNumber = cleaner.InternalRefCount + 1;
                }
                else if (string.Equals(nodeToAnalyze.Name,  HtmlConstants.HTML_DIV) &&
                         HtmlUtils.TryGetCodeSnipped(nodeToAnalyze, out CodeSectionInfo codeSectionInfo))
                {
                    // code
                    nodeToAnalyze.InnerHtml = codeSectionInfo.ToString();
                }
                else if (string.Equals(nodeToAnalyze.Name, HtmlConstants.HTML_DIV) 
                         && HtmlUtils.TryGetMetadataInfo(nodeToAnalyze, out string metadata))
                {
                    // metadata
                    nodeToAnalyze.InnerHtml = metadata;
                }
                else if (!nodeToAnalyze.HasChildNodes)
                {
                    // Remove multiples black spaces
                    nodeToAnalyze.InnerHtml = HtmlUtils.RemoveMultiplesSpaces(nodeToAnalyze.InnerHtml);
                }
                else if(string.Equals(nodeToAnalyze.Name,  HtmlConstants.HTML_SECTION) 
                        && HtmlUtils.IdentifyElementType(nodeToAnalyze) == HtmlSectionType.FooterSection)
                {  
                    // footer
                    elementsToRemove.Add(nodeToAnalyze);
                }
                else if (nodeToAnalyze.HasChildNodes)
                {
                    ApplyCustomConversions(nodeToAnalyze.ChildNodes);
                }
            }

            // remove invalid elements
            foreach (HtmlNode htmlNode in elementsToRemove)
            {
                nodes.Remove(htmlNode);
            }
        }

        private StringBuilder ReplaceLinks(ConfluencePageRef pageRef, StringBuilder originalText)
        {
            if (this._linkReferences.TryGetValue(pageRef.HtmlLocalFileName, out List<LinkElementInfo> links))
            {
                foreach (LinkElementInfo link in links)
                {
                    if (string.Equals(link.OriginalRef, "#"))
                    {
                        continue;
                    }

                    try
                    {
                        if (string.IsNullOrEmpty(link.OriginalRef))
                        {
                            continue;
                        }

                        switch (link.ResourceType)
                        {
                            case ResourceType.AttachmentLink:
                                link.NewRef = this.GetAttachmentUrl(link.OriginalRef);

                                if (string.IsNullOrEmpty(link.NewRef))
                                {
                                    NotifyProcess($"WARN: attachment resource not found '{link.OriginalRef}'");
                                    link.NewRef = "#";
                                }

                                originalText.Replace(link.OriginalRef, link.NewRef);

                                break;
                            case ResourceType.PageExistsOnWiki:

                                ConfluencePageRef pageInfo = GetPageRefFromMenu(link.OriginalRef);

                                if (pageInfo != null)
                                {
                                    originalText.Replace(link.OriginalRef, pageInfo.PagePathAtAzureDevOps);
                                    link.NewRef = pageInfo.PagePathAtAzureDevOps;
                                }
                                else
                                {
                                    NotifyProcess($"WARN: Reference to wiki page not found: '{link.OriginalRef}'");
                                    link.NewRef = "#";
                                }
                                
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        NotifyProcess($"WARN: Can't replace links: '{link.OriginalRef}', {e.Message}");
                    }
                }
            }


            return originalText;
        }

        private string GetAttachmentUrl(string actualRef)
        {
            string newLocation;

            Uri actualUri;

            if (!actualRef.StartsWith("http"))
            {
                actualUri = new Uri($"http://localhost/{actualRef}");
            }
            else
            {
                actualUri = new Uri(actualRef);
            }

            string absolutePath = actualUri.AbsolutePath;

            int queryStringStart = absolutePath.IndexOf("?", StringComparison.InvariantCulture);

            if (queryStringStart > 0)
            {
                absolutePath = absolutePath.Substring(0, queryStringStart);
            }

            var elements = new List<string>();
            elements.Add(_htmlSourceFolder);
            elements.AddRange(absolutePath.Split('/'));

            string originalFilePath = Path.Combine(elements.ToArray());

            string fileName = Path.GetFileName(absolutePath);

            newLocation = Path.Combine(WIKI_RESULT_ATTACHMENT_FILES, fileName);

            string finalAttachmentPath = Path.Combine(_resultDir, newLocation);

            if (File.Exists(originalFilePath))
            {
                if (!File.Exists(finalAttachmentPath))
                {
                    File.Copy(originalFilePath, finalAttachmentPath);
                }
            }
            else
            {
                newLocation = string.Empty;
            }

            return newLocation;
        }

        private ConfluencePageRef GetPageRefFromMenu(string actualRef)
        {
            ConfluencePageRef pageInfo = null;

            if (string.Equals(_wikiMenu.HtmlLocalFileName, actualRef))
            {
                pageInfo = _wikiMenu;
            }

            if (pageInfo == null)
            {
                pageInfo = GetPageInfoFromMenu(_wikiMenu.SubPages, actualRef);
            }

            // if (pageInfo == null)
            // {
            //     string posibleFileExists = Path.Combine(_htmlSourceFolder, actualRef);
            //
            //     if (File.Exists(posibleFileExists))
            //     {
            //         bool pageExists;
            //         var notInMenu = _wikiMenu.SubPages.FirstOrDefault(p => p.HtmlTitle == "NotInMenu");
            //
            //         pageExists = notInMenu != null;
            //
            //         if (!pageExists)
            //         {
            //             notInMenu = new ConfluencePageRef("Not ref", actualRef);
            //         }
            //
            //         //ConvertHtml2Markdown2();
            //         //ConfluencePageRef 
            //     }
            // }

            return pageInfo;
        }
        
        private ConfluencePageRef GetPageInfoFromMenu(List<ConfluencePageRef> subPages, string searchedPage)
        {
            ConfluencePageRef linkInfo = subPages.FirstOrDefault(p => string.Equals(p.HtmlLocalFileName, searchedPage));

            if (linkInfo == null)
            {
                foreach (ConfluencePageRef subPage in subPages)
                {
                    if (subPage.SubPages.Any())
                    {
                        linkInfo = GetPageInfoFromMenu(subPage.SubPages, searchedPage);

                        if (linkInfo != null)
                        {
                            break;
                        }
                    }
                }
            }

            return linkInfo;
        }

        private void ConvertHtml2Markdown2(ConfluencePageRef confluencePageRef, string htmlOuterDocument)
        {
            if (!string.IsNullOrEmpty(confluencePageRef.MarkdownLocalFilename))
            {
                var converter = new ReverseMarkdown.Converter();
                StringBuilder mdFileContent = new StringBuilder(converter.Convert(htmlOuterDocument));
                
                var valuesToReplace = new Dictionary<string, string>()
                {
                    {"<br>", " "},
                    {HtmlConstants.NEW_LINE, "\n"},
                    {HtmlConstants.BOLD_STILE, "**"},
                    {HtmlConstants.ITALIC_STILE, "_"}
                };
                
                foreach (string k in valuesToReplace.Keys)
                {
                    mdFileContent.Replace(k, valuesToReplace[k]);
                }
               
                mdFileContent.Append(
                    $"\n_Migrate on {DateTime.Now:f} with [Atlassian2DevOps Tool](https://github.com/etgregor/atlassian2devops)._");
                
                string fileDestinyFullPath = Path.Combine(_resultDir, confluencePageRef.MarkdownLocalFilename);

                string fileExists = IoUtils.GetPathIfFileExists(_resultDir, confluencePageRef.MarkdownLocalFilename);

                if (string.IsNullOrEmpty(fileExists))
                {
                    IoUtils.SaveFile(fileDestinyFullPath, mdFileContent.ToString());
                }
                else
                {
                    ProcessNotifier($"WARN: File already exists: {confluencePageRef.HtmlLocalFileName}  => {fileExists}");
                }
            }
        }
        
        #endregion

        #region - Utils -

        private void WriteJsonObject<T>(string fileName, T value)
        {
            string jsonFile = Path.Combine(_rootWorkingDir, fileName);
            
            IoUtils.SaveObjectToFile(jsonFile, value);

            Trace.TraceInformation($"Write file {jsonFile}");
        }
        
        private void NotifyProcess(string message)
        {
            ProcessNotifier?.Invoke(message);
        }

        #endregion
    }
}