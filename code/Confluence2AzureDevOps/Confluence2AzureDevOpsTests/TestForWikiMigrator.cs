using System;
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
        }
        
        [Test]
        public void StartMigrationTest()
        {
            try
            {    
                _wikiMigrator.StartMigration();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message + "" + e.StackTrace);
            }
        }
    }
}