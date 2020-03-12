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
        private string migrationId;

        private string outputConvertionMdFiles;
        
        private string outputAttachments;
            
        private Html2MdConverter _converter;
        
        private MigrationConfig _config;
        
        public WikiMigrator(MigrationConfig config)
        {
            _config = config;
            
            migrationId = $"Migration_{DateTime.Now:ddMMyy_HHmm}";

            outputConvertionMdFiles = Path.Combine(config.LocalConfig.LocalWorkspacePath, $"MD{migrationId}");
        }

        public void StartMigration(string confluenceIndexFile = "index.html", string selectorOfIndexControl = "//*[@id='content']/div[2]/ul")
        {
            _converter = new Html2MdConverter(_config.LocalConfig.LocalConfluencePath, outputConvertionMdFiles);
            _converter.ConvertHtmlToMdFiles(confluenceIndexFile, selectorOfIndexControl);
        }
    }
}