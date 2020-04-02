using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Confluence2AzureDevOps;
using Confluence2AzureDevOps.Base.CustomExceptions;
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
        public async Task StartMigrationTest()
        {
            try
            {
                bool success = await _wikiMigrator.MigrateWiki();

                Assert.IsTrue(success);
            }
            catch (ApiInvalidInputDataException dataException)
            {
                Assert.Fail(dataException.Message, dataException.Detail.Message);
            }
            catch (ApiException e1)
            {
                if (e1.Detail != null)
                {
                    _log.AppendLine("ERR1");
                    _log.AppendLine(e1.Message);
                    _log.AppendLine(e1.Detail.Message);
                    
                    Assert.Fail(e1.Message + "" + e1.Detail.Message);
                }
                else
                {
                    _log.AppendLine("ERR2");
                    _log.AppendLine(e1.Message);
                    _log.AppendLine(e1.StackTrace);
                    
                    Assert.Fail(e1.Message + "" + e1.StackTrace);    
                }
            }
            catch (Exception e)
            {   
                _log.AppendLine("ERR3");
                _log.AppendLine(e.Message);
                _log.AppendLine(e.StackTrace);

                foreach (DictionaryEntry val in e.Data)
                {
                    _log.Append($"{val.Key} => {val.Value}");
                }
                
                Assert.Fail(e.Message + "" + e.StackTrace + e.Data.Values);
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
            TestContext.Out.WriteLine(message);
            _log.AppendLine(message);
        }
    }
}