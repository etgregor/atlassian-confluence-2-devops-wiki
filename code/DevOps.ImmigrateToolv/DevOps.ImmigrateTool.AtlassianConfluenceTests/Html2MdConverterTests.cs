using System;
using DevOps.ImmigrateTool.AtlasianConfluenceTests.Utils;
using DevOps.ImmigrateTool.AtlassianConfluence.DevOpsApiClient;
using DevOps.ImmigrateTool.AtlassianConfluence.Processor;
using NUnit.Framework;

namespace DevOps.ImmigrateTool.AtlasianConfluenceTests
{
    public class Html2MdConverterTests
    {
        private Html2MdConverter _converter;
        
        [SetUp]
        public void Setup()
        {
            // Check internal code for test init.
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