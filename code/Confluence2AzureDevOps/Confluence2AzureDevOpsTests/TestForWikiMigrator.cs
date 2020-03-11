using Confluence2AzureDevOps;
using Confluence2AzureDevOps.ObjectModel;
using Confluence2AzureDevOpsTests.UtilsForTesting;
using NUnit.Framework;

namespace Confluence2AzureDevOpsTests
{
    public class TestForWikiMigrator
    {
        private WikiMigrator _wikiMigrator;
        
        [SetUp]
        public void Setup()
        {
            var config = new MigrationConfig();

            #region - Local config -

            LocalSettingTests localSetting = TestUtils.GetLocalSettings();
            
            config.LocalConfig.LocalConfluencePath = localSetting.LocalWikiSourceFolder;
            
            config.LocalConfig.LocalWorkspacePath = localSetting.LocalWikiDestinatioFolder;

            #endregion

            #region - Azure devops Config -

            DevOpsSettingsTests devOpsSettings = TestUtils.GetDevopsTestSettings();
            config.AzureDevOpsConfig.Organization = devOpsSettings.Organization;
            config.AzureDevOpsConfig.Project = devOpsSettings.Project;
            config.AzureDevOpsConfig.WikiIdentifier = devOpsSettings.WikiIdentifier;
            config.AzureDevOpsConfig.PersonalAccessToken = devOpsSettings.PersonalAccesToken;
            
            #endregion
            
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