using System;
using System.IO;
using System.Threading.Tasks;
using Confluence2AzureDevOps.DevOpsApiClient;
using Confluence2AzureDevOpsTests.UtilsForTesting;
using NUnit.Framework;

namespace Confluence2AzureDevOpsTests.TestsForDevOpsApiClients
{
    public class TestForAttachmentsApi
    {
        private AttachmentsApi _target;
        
        [SetUp]
        public void Setup()
        {
            // Check internal code for test init.
            DevOpsSettingsTests devOpsSetting = TestUtils.GetDevopsTestSettings();

            _target = new AttachmentsApi(
                devOpsSetting.Organization, 
                devOpsSetting.Project, 
                devOpsSetting.WikiIdentifier, 
                devOpsSetting.PersonalAccesToken);
        }
        
        [Test]
        public async Task UploadFileTest()
        {
            var file =
                "/Users/McGregor/Downloads/formiik/backup_confluence/wiki/FD-MD/attachments/832536597/832569372.png";

            var fileContent = File.ReadAllBytes(file);

            var wikiPage = await _target.UploadFile("TestFile1.png", fileContent);
            
            Assert.IsNotNull(wikiPage);

            Console.WriteLine(wikiPage);
        }
    }
}