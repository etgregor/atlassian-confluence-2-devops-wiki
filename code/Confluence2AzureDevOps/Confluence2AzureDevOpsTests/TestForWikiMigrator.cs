using System;
using System.IO;
using System.Text;
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
        private StringBuilder _log;
        private MigrationConfig _migrationConfig;
        
        [SetUp]
        public void Setup()
        {
            _log = new StringBuilder();
            
            _migrationConfig = TestUtils.GetMigrationConfigTest();

            _wikiMigrator = new WikiMigrator(_migrationConfig)
            {
                ProcessNotifier = WriteProcess
            };
        }
        
        [Test]
        public void StartMigrationTest()
        {
            try
            {    
                _wikiMigrator.StartMigration();
                
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message + "" + e.StackTrace);
            }
        }

        [TearDown]
        public void FinalizeTest()
        {
            if (_wikiMigrator != null && _log.Length > 0)
            {
                if (Directory.Exists(_wikiMigrator.OutputDir))
                {
                    string logPath = Path.Combine(_wikiMigrator.OutputDir, "log.txt");

                    File.WriteAllText(logPath, _log.ToString());
                }
            }
        }

        private void WriteProcess(string message)
        {
            _log.AppendLine(message);
        }
    }
}