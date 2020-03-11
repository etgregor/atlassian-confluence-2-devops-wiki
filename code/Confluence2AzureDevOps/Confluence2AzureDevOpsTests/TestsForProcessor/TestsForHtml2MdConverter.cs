using System;
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

            _converter = new Html2MdConverter(setting.LocalWikiSourceFolder, setting.LocalWikiDestinatioFolder);
        }

        [Test]
        public void ConvertHtml2MdFilesTests()
        {
            try
            {    
                _converter.StartConvertion();
                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}