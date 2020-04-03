using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Confluence2AzureDevOps.ObjectModel;
using Confluence2AzureDevOps.ObjectModel.HtmlElements;
using Confluence2AzureDevOps.Processor;

namespace Confluence2AzureDevOps
{
    /// <summary>
    /// Migrate 
    /// </summary>
    public class WikiMigrator
    {
        private readonly string _migrationId;

        private readonly string _outputConversionMdFiles;
            
        private Html2MdConverter _converter;
        
        private readonly MigrationConfig _config;

        private DevOpsWikiUploader _wikiUploader;
        
        public BmlProcessNotifier ProcessNotifier { get; set; }

        public string OutputDir
        {
            get { return _outputConversionMdFiles; }
        }

        public WikiMigrator(MigrationConfig config)
        {
            _config = config;
            
            _migrationId = $"Migration_{DateTime.Now:ddMMyy_HHmm}";

            _outputConversionMdFiles = Path.Combine(_config.LocalConfig.LocalWorkspacePath, $"{_migrationId}");
        }

        public async Task<bool> MigrateWiki(string rootPageNameForWiki = "Home", string confluenceIndexFile = "index.html", string selectorOfIndexControl = "//*[@id='content']/div[2]/ul")
        {
            _converter = new Html2MdConverter(_config.LocalConfig.LocalConfluencePath, _outputConversionMdFiles, _config.ReplazableTitles);
            _converter.ProcessNotifier = NotifyProcess;
            
            ConfluencePageRef mapSite = _converter.ConvertHtmlToMdFiles(
                defaultPage:rootPageNameForWiki, 
                confluenceIndexFile: confluenceIndexFile, 
                selectorOfIndexControl: selectorOfIndexControl);
            Dictionary<string, List<LinkElementInfo>> attachments = _converter.AttachmentsFiles;
            
            _wikiUploader = new DevOpsWikiUploader(_config, _outputConversionMdFiles);
            _wikiUploader.ProcessNotifier = NotifyProcess;
            bool success = await _wikiUploader.UploadWiki(mapSite, attachments);
            
            return success;
        }
        
        private void NotifyProcess(string message)
        {
            ProcessNotifier?.Invoke(message);
        }
    }
}