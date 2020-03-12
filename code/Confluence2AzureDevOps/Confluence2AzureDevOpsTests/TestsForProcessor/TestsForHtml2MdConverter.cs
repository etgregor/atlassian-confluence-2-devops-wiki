using System;
using System.IO;
using Confluence2AzureDevOps.ObjectModel;
using Confluence2AzureDevOps.Processor;
using Confluence2AzureDevOpsTests.UtilsForTesting;
using NUnit.Framework;

namespace Confluence2AzureDevOpsTests.TestsForProcessor
{
    public class TestsForHtml2MdConverter
    {
        private Html2MdConverter _converter;
        
        [SetUp]
        public void Setup()
        {
            // Init test values.
            LocalSettingTests setting = TestUtils.GetLocalSettings();

            string testTransformId = $"TestTransform_{DateTime.Now:ddMMyyHHmmss}";

            string workingDir =  Path.Combine(setting.LocalWikiDestinatioFolder, testTransformId);

            _converter = new Html2MdConverter(setting.LocalWikiSourceFolder, workingDir);

            _converter.ProcessNotifier = WriteProcess;
        }

        [Test]
        public void ConvertHtml2MdFilesTests()
        {
            try
            {    
                ConfluencePageRef siteMapIndex =  _converter.ConvertHtmlToMdFiles();
                Assert.IsNotNull(siteMapIndex);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        private void WriteProcess(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}