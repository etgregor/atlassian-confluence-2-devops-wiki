using NUnit.Framework;

namespace Confluence2AzureDevOpsTests
{
    public class TestsForWikiMigratorHtmlToMd
    {
        // private PagesApi _target;
        private string _localWikiSourceFolder;
        
        private string _localWikiDestinatioFolder;

        [SetUp]
        public void Setup()
        {
            _localWikiSourceFolder = TestContext.Parameters["localWikiSourceFolder"];
            _localWikiDestinatioFolder = TestContext.Parameters["localWikiDestinatioFolder"];

//            _project = TestContext.Parameters["project"];
//            
//            _wikiIdentifier = TestContext.Parameters["wikiIdentifier"];
//            
//            _personalAccesToken = TestContext.Parameters["personalAccesToken"];

//            _target = new PagesApi(_organization, _project, _wikiIdentifier, _personalAccesToken);
        }
    }
}