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
        
        /// <summary>
        /// Max length for path at azure dev ops, <see cref="https://docs.microsoft.com/en-us/azure/devops/project/wiki/wiki-file-structure?view=azure-devops"/> 
        /// </summary>
        private const int AZURE_DEV_OPS_MAX_PATH_LENGTH = 235;
        
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

                if (wikiPageInfo.PagePathAtAzureDevOps.Length > AZURE_DEV_OPS_MAX_PATH_LENGTH)
                {
                    NotifyProcess($"WARN: Too long path ({AZURE_DEV_OPS_MAX_PATH_LENGTH}): {wikiPageInfo.PagePathAtAzureDevOps}");
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

        public void PrepareHtmlFile(ConfluencePageRef confluencePageRef)
        {
            HtmlDocument htmlPage = ReadOriginalFileAsHtml(confluencePageRef.HtmlLocalFileName);
    
            if (htmlPage != null)
            {
                ApplyCustomConversions(confluencePageRef,  htmlPage.DocumentNode.ChildNodes);
                
                List<LinkElementInfo> linksOnPage = GetLinksElement(htmlPage.DocumentNode.ChildNodes);
                
                _linkReferences.Add(confluencePageRef.HtmlLocalFileName, linksOnPage);

                string preprocessedHtml = Path.Combine(_processedHtmlDir, confluencePageRef.HtmlLocalFileName);
                
                htmlPage.Save(preprocessedHtml);

                ConvertHtml2Markdown2(confluencePageRef, htmlPage.DocumentNode.OuterHtml);
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

        private void ApplyCustomConversions(ConfluencePageRef confluencePageRef, HtmlNodeCollection nodes)
        {
            var elementsToRemove = new List<HtmlNode>();

            foreach (HtmlNode child in nodes)
            {
                if (string.Equals(child.Name, HtmlConstants.HTML_SCRIPT)  
                    || string.Equals(child.Name, HtmlConstants.HTML_HEAD))
                {
                    elementsToRemove.Add(child);
                }
                else if (string.Equals(child.Name, HtmlConstants.HTML_DIV) 
                         && HtmlUtils.ContainsTableElement(child, out HtmlNode tableNode))
                {
                    // Process element only if is unique child
                    //HtmlNode tableNode = child.FirstChild;
                    var cleaner = new HtmlTableCleaner(tableNode);
                    child.InnerHtml = cleaner.GetTableDefinition();
                }
                else if (string.Equals(child.Name,  HtmlConstants.HTML_DIV) &&
                         HtmlUtils.TryGetCodeSnipped(child, out CodeSectionInfo codeSectionInfo))
                {
                    // Formatting code sections
                    child.InnerHtml = codeSectionInfo.ToString();
                }
                else if (string.Equals(child.Name, HtmlConstants.HTML_DIV) 
                         && HtmlUtils.TryGetMetadataInfo(child, out string metadata))
                {
                    // replace metadata file
                    child.InnerHtml = metadata;
                }
                else if (!child.HasChildNodes)
                {
                    // Remove multiples black spaces
                    child.InnerHtml = child.InnerHtml.Trim();
                }
                else if (child.HasChildNodes)
                {
                    ApplyCustomConversions(confluencePageRef, child.ChildNodes);
                }
            }

            // remove invalid elements
            foreach (HtmlNode htmlNode in elementsToRemove)
            {
                nodes.Remove(htmlNode);
            }
        }
        
        #endregion
       
        private void ConvertHtml2Markdown2(ConfluencePageRef confluencePageRef, string htmlOuterDocument)
        {
            if (!string.IsNullOrEmpty(confluencePageRef.MarkdownLocalFilename))
            {
                var converter = new ReverseMarkdown.Converter();
                
                // replace special chars
                string mdFileContent = converter.Convert(htmlOuterDocument);
                
                mdFileContent = mdFileContent.Replace("<br>", " ");
                mdFileContent = mdFileContent.Replace(HtmlConstants.NEW_LINE, "\n");
                mdFileContent = mdFileContent.Replace(HtmlConstants.BOLD_STILE, "**");
                mdFileContent = mdFileContent.Replace(HtmlConstants.ITALIC_STILE, "_");
                // mdFileContent = mdFileContent.Replace(HtmlConstants.INTERNAL_LINK_TO_SECTION, "#");
                // mdFileContent = mdFileContent.Replace(HtmlConstants.INTERNAL_SECTION, "#####");
                // mdFileContent = HtmlUtils.RemoveMultiplesSpaces(mdFileContent);
                
                string fileDestinyFullPath = Path.Combine(_resultDir, confluencePageRef.MarkdownLocalFilename);

                string fileExists = IoUtils.GetPathIfFileExists(_resultDir, confluencePageRef.MarkdownLocalFilename);

                if (string.IsNullOrEmpty(fileExists))
                {
                    IoUtils.SaveFile(fileDestinyFullPath, mdFileContent);
                }
                else
                {
                    ProcessNotifier($"WARN: File already exists: {confluencePageRef.HtmlLocalFileName}  => {fileExists}");
                }
            }
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