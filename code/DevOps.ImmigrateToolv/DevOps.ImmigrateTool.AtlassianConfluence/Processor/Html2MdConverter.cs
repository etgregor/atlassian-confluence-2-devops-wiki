using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using DevOps.ImmigrateTool.AtlassianConfluence.Base.CustomExceptions;
using DevOps.ImmigrateTool.AtlassianConfluence.Entities.WikiPages;
using DevOps.ImmigrateTool.AtlassianConfluence.Utils;
using HtmlAgilityPack;

namespace DevOps.ImmigrateTool.AtlassianConfluence.Processor
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
            ConfluencePageRef pageRef = ReadIndexOfPages();
            string jsonFile = Path.Combine(_mdDestinationFolder, "jsonIndex.json");
            IoUtils.SaveObjectToFile(jsonFile, pageRef);

            System.Diagnostics.Debug.WriteLine($"Write file {jsonFile}");
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
                                confluencePageRef.Title = value;
                                confluencePageRef.File = fileName;

                                lastPage = confluencePageRef;
                            }
                            else
                            {
                                lastPage = new ConfluencePageRef();
                                lastPage.Title = value;
                                lastPage.File = fileName;

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