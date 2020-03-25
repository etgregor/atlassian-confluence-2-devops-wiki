using System;
using System.IO;
using System.Text;
using Confluence2AzureDevOps.ObjectModel;
using Confluence2AzureDevOps.Processor;
using Confluence2AzureDevOpsTests.UtilsForTesting;
using NUnit.Framework;

namespace Confluence2AzureDevOpsTests.TestsForProcessor
{
    [TestFixture]
    public class TestsPartForHtml2MdConverter
    {
        private Html2MdConverter _converter;

        private StringBuilder _log;

        private string _workingDir;

        [SetUp]
        public void Setup()
        {
            _log = new StringBuilder();

            // Init test values.
            MigrationConfig config = TestUtils.GetMigrationConfigTest();

            _workingDir = Path.Combine(config.LocalConfig.LocalWorkspacePath, "TestTransform_130320153043");

            _converter = new Html2MdConverter(config.LocalConfig.LocalConfluencePath, _workingDir);

            _converter.ProcessNotifier = WriteProcess;
        }

        [Test]
        public void TestsForPrepareHtmlFile()
        {
            ConfluencePageRef conf = new ConfluencePageRef()
            {
                HtmlTitle = "1. NewOrder - Estructura XML",
                HtmlLocalFileName = "1.-NewOrder---Estructura-XML_29261850.html",
                MarkdownLocalFilename = "1_ NewOrder - Estructura XML.md",
                PageTitleAtAzureDevOps = "1_ NewOrder - Estructura XML",
                PagePathAtAzureDevOps =
                    "/Home/Documentacion Tecnica/Articulos tecnicos/Funcionamiento/Servicios Formiik/Servicios Web que Expone Formiik (Backend)/BackEnd_svc/Administrar Ordenes/1. NewOrder - Estructura XML",
            };
            
            System.IO.File.Delete($"{_workingDir}/3_ResultWikiMd/1_ NewOrder - Estructura XML.md");

            _converter.ProcessHtmlFile(conf);
        }

        private void WriteProcess(string message)
        {
            _log.AppendLine(message);
        }
    }
}