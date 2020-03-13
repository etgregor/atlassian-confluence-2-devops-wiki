using Confluence2AzureDevOps;
using Confluence2AzureDevOps.ObjectModel;
using Confluence2AzureDevOpsTests.UtilsForTesting;
using NUnit.Framework;

namespace Confluence2AzureDevOpsTests
{
    [TestFixture]
    public class TestForWikiMigrator
    {
        private WikiMigrator _wikiMigrator;
        
        [SetUp]
        public void Setup()
        {
            MigrationConfig config = TestUtils.GetMigrationConfigTest();
            
            _wikiMigrator = new WikiMigrator(config);
            
            _wikiMigrator.StartMigration();
            
            // Init test values.
            //LocalSettingTests setting = TestUtils.GetLocalSettings();

//            _project = TestContext.Parameters["project"];
//            
//            _wikiIdentifier = TestContext.Parameters["wikiIdentifier"];
//            
//            _personalAccesToken = TestContext.Parameters["personalAccesToken"];

//            _target = new PagesApi(_organization, _project, _wikiIdentifier, _personalAccesToken);
        }
    }
}