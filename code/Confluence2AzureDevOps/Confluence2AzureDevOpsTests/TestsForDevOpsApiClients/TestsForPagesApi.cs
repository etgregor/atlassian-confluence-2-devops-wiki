using System;
using System.Threading.Tasks;
using Confluence2AzureDevOps.DevOpsApiClient;
using Confluence2AzureDevOps.Entities.WikiPages;
using Confluence2AzureDevOpsTests.UtilsForTesting;
using NUnit.Framework;

namespace Confluence2AzureDevOpsTests.TestsForDevOpsApiClients
{
    public class TestsForPagesApi
    {
        private PagesApi _target;
        
        [SetUp]
        public void Setup()
        {
            // Check internal code for test init.
            DevOpsSettingsTests devOpsSetting = TestUtils.GetDevopsTestSettings();

            _target = new PagesApi(
                devOpsSetting.Organization, 
                devOpsSetting.Project, 
                devOpsSetting.WikiIdentifier, 
                devOpsSetting.PersonalAccesToken);
        }

        [Test]
        public async Task GetFullTree_Test()
        {
            DtWikiPage wikiPage = await _target.GetFullTree();
            
            Assert.IsNotNull(wikiPage);
        }
        
        /// <summary>
        /// Create page 
        /// </summary>
        [Test]
        public async Task CreateOrUpdatePage_Test()
        {
            var page = new PageWikiCreateOptions();
            page.Comment = string.Format($"Page created from unit test: {DateTime.Now:dd-MMM-yy-HHmmss}");

            page.Path = $"UT Sample {DateTime.Now:dd-MM-yy-HHmmss}";

            // page.Content = "Hello world!!";
            string fileContent =
                TestFileUtil.ReadContentFromResource("Resources", "ExampleFile.md");
            page.Content = fileContent;      
            
            DtWikiPage wikiPage = await _target.CreateOrUpdatePage(page);
            
            Assert.IsNotNull(wikiPage);
        }
    }
}