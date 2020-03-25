using System;
using System.IO;
using Confluence2AzureDevOps.ObjectModel;
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
        
        public BmlProcessNotifier ProcessNotifier { get; set; }

        public string OutputDir
        {
            get { return _outputConversionMdFiles; }
        }

        public WikiMigrator(MigrationConfig config)
        {
            _config = config;
            
            _migrationId = $"Migration_{DateTime.Now:ddMMyy_HHmm}";

            _outputConversionMdFiles = Path.Combine(config.LocalConfig.LocalWorkspacePath, $"{_migrationId}");
        }

        public void StartMigration(string confluenceIndexFile = "index.html", string selectorOfIndexControl = "//*[@id='content']/div[2]/ul")
        {
            _converter = new Html2MdConverter(_config.LocalConfig.LocalConfluencePath, _outputConversionMdFiles, _config.ReplazableTitles);
            _converter.ProcessNotifier = NotifyProcess;
            
            _converter.ConvertHtmlToMdFiles(confluenceIndexFile, selectorOfIndexControl);
        }
        
        private void NotifyProcess(string message)
        {
            ProcessNotifier?.Invoke(message);
        }
    }
}