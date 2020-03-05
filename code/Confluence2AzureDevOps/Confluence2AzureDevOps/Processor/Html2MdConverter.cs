using System;
using System.IO;
using System.Linq;
using System.Text;
using Confluence2AzureDevOps.Base.CustomExceptions;
using Confluence2AzureDevOps.Entities.WikiPages;
using Confluence2AzureDevOps.Utils;
using HtmlAgilityPack;

namespace Confluence2AzureDevOps.Processor
{
    public class Html2MdConverter
    {
        private string _htmlSourceFolder;
        private string _mdDestinationFolder;

        private const string INDEX_FILE = "index.html";

        public Html2MdConverter(string htmlSourceFolder, string mdDestinationFolder)
        {
            _htmlSourceFolder = htmlSourceFolder;
            _mdDestinationFolder = mdDestinationFolder;

            Guard.PreventStringEmpty("htmlSourceFolder", htmlSourceFolder);
            Guard.PreventStringEmpty("mdDestinationFolder", mdDestinationFolder);
            Guard.PreventDirectoryNotExistt(htmlSourceFolder);

            IoUtils.CreateFolderPath(mdDestinationFolder);
        }

        public void StartConvertion()
        {
            ConfluencePageRef wikiMenu = ReadIndexOfPages();
            
            string jsonFile = Path.Combine(_mdDestinationFolder, "jsonIndex.json");
            
            IoUtils.SaveObjectToFile(jsonFile, wikiMenu);

            System.Diagnostics.Debug.WriteLine($"Write file {jsonFile}");

            ConvertHtml2Markdown(wikiMenu);
        }

        private ConfluencePageRef ReadIndexOfPages()
        {
            ConfluencePageRef mainPage = null;

            string indexFilePath = System.IO.Path.Combine(_htmlSourceFolder, INDEX_FILE);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.Load(indexFilePath);

            // ParseErrors is an ArrayList containing any errors from the Load statement
            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
            {
                var errors = new StringBuilder();

                foreach (HtmlParseError error in htmlDoc.ParseErrors)
                {
                    errors.AppendLine(error.Reason);
                }

                throw new GenericException(errors.ToString());
            }

            if (htmlDoc.DocumentNode != null)
            {
                HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='content']/div[2]/ul");

                if (bodyNode != null)
                {   
                    GetPageInfo(ref mainPage, bodyNode);
                }
            }

            return mainPage;
        }

        private void ConvertHtml2Markdown(ConfluencePageRef siteIndex)
        {
            System.Diagnostics.Debug.WriteLine($"Start conversion file: {siteIndex.HtmlLocalFileName}");
            string markdownFileName = ConvertFile(siteIndex.HtmlLocalFileName);

            if (!string.IsNullOrEmpty(markdownFileName))
            {
                System.Diagnostics.Debug.WriteLine($"New file created: {markdownFileName}");
                
                siteIndex.MarkdownLocalFileName = markdownFileName;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"WARN: Failed convertion file: {siteIndex.HtmlLocalFileName}");
            }

            foreach (ConfluencePageRef subPage in siteIndex.SubPages)
            {
                ConvertHtml2Markdown(subPage);
            }
        }

        private string ConvertFile(string htmlFileName)
        {
            string markdownFileName = string.Empty;

            var filePath = IoUtils.GetPathIfFileExists(_htmlSourceFolder, htmlFileName);

            if (!string.IsNullOrEmpty(filePath))
            {
                string htmlFileContent = IoUtils.ReadFileContent(filePath);
                
                var converter = new ReverseMarkdown.Converter();

                string result = converter.Convert(htmlFileContent);

                markdownFileName = string.Format("{0}.md", Path.GetFileNameWithoutExtension(htmlFileName));
                
                string fileDestinyFullPath = System.IO.Path.Combine(_mdDestinationFolder, markdownFileName);
                
                IoUtils.SaveFile(fileDestinyFullPath, result);
            }

            return markdownFileName;
        }
        
        
        private void GetPageInfo(ref ConfluencePageRef confluencePageRef, HtmlNode bodyNode )
        {
            ConfluencePageRef lastPage = null;
                
            foreach (HtmlNode child in bodyNode.ChildNodes.Descendants())
            {
                if (child.Name == "a")
                {
                    try
                    {
                        string fileName = child.GetAttributeValue("href", string.Empty);

                        string value = child.InnerText;

                        if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(value))
                        {
                            if (confluencePageRef == null)
                            {
                                confluencePageRef = new ConfluencePageRef();
                                confluencePageRef.HtmlTitle = value;
                                confluencePageRef.HtmlLocalFileName = fileName;

                                lastPage = confluencePageRef;
                            }
                            else
                            {
                                lastPage = new ConfluencePageRef();
                                lastPage.HtmlTitle = value;
                                lastPage.HtmlLocalFileName = fileName;

                                confluencePageRef.SubPages.Add(lastPage);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else if (child.HasChildNodes)
                {
                    GetPageInfo(ref lastPage, child);
                }
            }
        }
    }
}