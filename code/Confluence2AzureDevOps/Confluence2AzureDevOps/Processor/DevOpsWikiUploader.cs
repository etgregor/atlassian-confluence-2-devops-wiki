using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Confluence2AzureDevOps.Base.CustomExceptions;
using Confluence2AzureDevOps.DevOpsApiClient;
using Confluence2AzureDevOps.ObjectModel;
using Confluence2AzureDevOps.ObjectModel.HtmlElements;
using Confluence2AzureDevOps.ObjectModel.WikiPages;

namespace Confluence2AzureDevOps.Processor
{
    public class DevOpsWikiUploader
    {
        public BmlProcessNotifier ProcessNotifier { get; set; }
        
        //private MigrationConfig _config;
        // private ConfluencePageRef _mapSite;
        private Dictionary<string, List<LinkElementInfo>> _attachments;
        
        private PagesApi _pagesApi;
        
        private AttachmentsApi _attachmentsApi;

        private string _workingDir;

        public DevOpsWikiUploader(MigrationConfig config, string workingDir)
        {
            //_config = config;
            _workingDir = workingDir;
            
            _attachmentsApi = new AttachmentsApi(
                config.AzureDevOpsConfig.Organization, 
                config.AzureDevOpsConfig.Project, 
                config.AzureDevOpsConfig.WikiIdentifier, 
                config.AzureDevOpsConfig.PersonalAccessToken);
            
            _pagesApi = new PagesApi(
                config.AzureDevOpsConfig.Organization,
                config.AzureDevOpsConfig.Project,
                config.AzureDevOpsConfig.WikiIdentifier,
                config.AzureDevOpsConfig.PersonalAccessToken);
        }

        public async Task<bool> UploadWiki(ConfluencePageRef mapSite,
            Dictionary<string, List<LinkElementInfo>> attachments)
        {
            _attachments = attachments;

            await UploadPage(mapSite);

            return true;
        }

        private async Task UploadPage(ConfluencePageRef pageToUpload)
        {
            NotifyProcess($"---->> Uploading: {pageToUpload.HtmlLocalFileName}");
            NotifyProcess($"{pageToUpload.PagePathAtAzureDevOps} => {pageToUpload.PageTitleAtAzureDevOps}");

            var page = new PageWikiCreateOptions();
            page.Comment = string.Format($"Page created by Gregorio Marciano using Confluence2AzureDevOps");
            page.Path = pageToUpload.PagePathAtAzureDevOps;

            DtWikiPage actualPage = await GetExistsPage(pageToUpload.PagePathAtAzureDevOps);

            if (actualPage == null)
            {
                string localFile = Path.Combine(_workingDir, "3_ResultWikiMd", pageToUpload.MarkdownLocalFilename);

                if (File.Exists(localFile))
                {
                    string mdContent = File.ReadAllText(localFile);

                    page.Content = mdContent;

                    DtWikiPage wikiPage = await _pagesApi.CreateOrUpdatePage(page);
                    
                    pageToUpload.SetMigrationSuccess(wikiPage.Path);
                }
            }
            else
            {
                NotifyProcess($"Already exists.");
            }
            
            try
            {
                if (_attachments.TryGetValue(pageToUpload.HtmlLocalFileName, out List<LinkElementInfo> links))
                {
                    await UploadFiles(links);
                }
            }
            catch (Exception e)
            {
                NotifyProcess($"Upload attachments fail {e.Message}");
            }
            
            if (pageToUpload.SubPages != null && pageToUpload.SubPages.Any())
            {
                NotifyProcess("Uploading attachments");

                foreach (ConfluencePageRef subPage in pageToUpload.SubPages)
                {
                    await UploadPage(subPage);
                }
            }
            else
            {
                NotifyProcess("Page not contains attachments");
            }
        }

        private async Task<DtWikiPage> GetExistsPage(string path)
        {
            DtWikiPage page = null;

            try
            {
                page = await _pagesApi.GetPage(path);
            }
            catch (ApiException e1)
            {
                if (e1.Detail != null 
                    && (
                        string.Equals(e1.Detail.TypeKey, "WikiPageNotFoundException") || 
                        string.Equals(e1.Detail.TypeKey, "WikiAncestorPageNotFoundException")))
                {
                    page = null;
                }
                else
                {
                    throw;
                }
            }

            return page;
        }
        
        
        private async Task UploadFiles(List<LinkElementInfo> links)
        {
            foreach (LinkElementInfo linkElementInfo in links)
            {
                if (linkElementInfo.ResourceType == ResourceType.AttachmentLink
                    && !string.IsNullOrEmpty(linkElementInfo.NewRef)
                    && !string.Equals(linkElementInfo.NewRef, "#"))
                {
                    await UploadAttachment(linkElementInfo);
                }
            }
        }

        private async Task UploadAttachment(LinkElementInfo linkElementInfo)
        {
            string filePath = Path.Combine(
                _workingDir,
                "3_ResultWikiMd",
                linkElementInfo.NewRef);
            
            try
            {
                if (File.Exists(filePath))
                {
                    byte[] fileContent = File.ReadAllBytes(filePath);

                    string fileName = Path.GetFileName(linkElementInfo.NewRef);

                    DtWikiAttachment wikiAttachment = await _attachmentsApi.UploadFile(fileName, fileContent);

                    linkElementInfo.SetMigrationMessage(wikiAttachment.Path);
                }
            }
            catch (Exception e)
            {
                NotifyProcess($"WARN {filePath}: {e.Message}");
            }
        }

        private void NotifyProcess(string message)
        {
            ProcessNotifier?.Invoke(message);
        }
    }
}