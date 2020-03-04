using System;
using System.Threading.Tasks;
using DevOps.ImmigrateTool.AtlasianConfluenceTests.Utils;
using DevOps.ImmigrateTool.AtlassianConfluence.DevOpsApiClient;
using DevOps.ImmigrateTool.AtlassianConfluence.Entities.WikiPages;
using NUnit.Framework;

namespace DevOps.ImmigrateTool.AtlasianConfluenceTests
{
    public class PagesApiTests
    {
        private string _organization;
        private string _project;
        private string _wikiIdentifier;
        private string _personalAccesToken;
        
        private PagesApi _target;
        
        [SetUp]
        public void Setup()
        {
            _organization = TestContext.Parameters["organization"];
            
            _project = TestContext.Parameters["project"];
            
            _wikiIdentifier = TestContext.Parameters["wikiIdentifier"];
            
            _personalAccesToken = TestContext.Parameters["personalAccesToken"];

            _target = new PagesApi(_organization, _project, _wikiIdentifier, _personalAccesToken);
        }

        [Test]
        public async Task GetFullTree_Test()
        {
            WikiPage wikiPage = await _target.GetFullTree();
            
            Assert.IsNotNull(wikiPage);
        }
        
        [Test]
        public async Task CreateOrUpdatePage_Test()
        {
            var page = new PageWikiCreateOptions();
            page.Comment = string.Format($"Page created from unit test: {DateTime.Now}");

            page.Path = $"UT Sample {DateTime.Now.ToString("dd-MM-yy-HHmmss")}";

            // page.Content = "Hello world!!";
            
            string fileContent =
                FileUtil.ReadContentFromResource("Resources", "ExampleFile.md");
            page.Content = fileContent;      
            
            WikiPage wikiPage = await _target.CreateOrUpdatePage(page);
            
            Assert.IsNotNull(wikiPage);
        }
    }
}