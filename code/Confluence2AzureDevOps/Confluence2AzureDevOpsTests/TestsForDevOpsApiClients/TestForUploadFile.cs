using System;
using System.IO;
using System.Threading.Tasks;
using Confluence2AzureDevOps.DevOpsApiClient;
using Confluence2AzureDevOps.ObjectModel;
using Confluence2AzureDevOps.ObjectModel.WikiPages;
using Confluence2AzureDevOpsTests.UtilsForTesting;
using NUnit.Framework;

namespace Confluence2AzureDevOpsTests.TestsForDevOpsApiClients
{
    [TestFixture]
    public class TestForAttachmentsApi
    {
        private AttachmentsApi _target;
        
        [SetUp]
        public void Setup()
        {
            // Check internal code for test init.
            MigrationConfig config = TestUtils.GetMigrationConfigTest();

            _target = new AttachmentsApi(
                config.AzureDevOpsConfig.Organization, 
                config.AzureDevOpsConfig.Project, 
                config.AzureDevOpsConfig.WikiIdentifier, 
                config.AzureDevOpsConfig.PersonalAccessToken);
        }
        
        [Test]
        public async Task UploadFileTest()
        {
            string fileName = "diagram example 123.png";
            
            var fileContent = TestFileUtil.ReadFile("Resources", fileName);
            
            DtWikiAttachment wikiPage = await _target.UploadFile(fileName, fileContent);
            
            Assert.IsNotNull(wikiPage);

            bool namesAreEquals = string.Equals(fileName, wikiPage.Name);
            
            Assert.True(namesAreEquals);

            Console.WriteLine(wikiPage);
        }
    }
}