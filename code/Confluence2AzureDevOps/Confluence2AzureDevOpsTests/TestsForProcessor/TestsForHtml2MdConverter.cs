using System;
using System.IO;
using System.Text;
using Confluence2AzureDevOps.ObjectModel;
using Confluence2AzureDevOps.Processor;
using Confluence2AzureDevOpsTests.UtilsForTesting;
using NUnit.Framework;

namespace Confluence2AzureDevOpsTests.TestsForProcessor
{
    [TestFixture]
    public class TestsForHtml2MdConverter
    {
        private Html2MdConverter _converter;

        private StringBuilder _log;

        private string _testTransformId;
        
        private string _workingDir;
        
        [SetUp]
        public void Setup()
        {
            _log = new StringBuilder();
            
            // Init test values.
            MigrationConfig config = TestUtils.GetMigrationConfigTest();
            
            _testTransformId = $"TestTransform_{DateTime.Now:ddMMyyHHmmss}";

            _workingDir =  Path.Combine(config.LocalConfig.LocalWorkspacePath, _testTransformId);
            
            _converter = new Html2MdConverter(config.LocalConfig.LocalConfluencePath, _workingDir);

            _converter.ProcessNotifier = WriteProcess;
        }

        [Test]
        public void ConvertHtml2MdFilesTests()
        {
            try
            {
                ConfluencePageRef siteMapIndex = _converter.ConvertHtmlToMdFiles(defaultPage: "MigrationTest");
                Assert.IsNotNull(siteMapIndex);

                string logPath = Path.Combine(_workingDir, "Log.txt");
                
                using (var writer = File.CreateText(logPath))
                {
                    writer.WriteLine(_log.ToString());
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message + "" + e.StackTrace);
            }
        }

        private void WriteProcess(string message)
        {
            _log.AppendLine(message);
        }
    }
}