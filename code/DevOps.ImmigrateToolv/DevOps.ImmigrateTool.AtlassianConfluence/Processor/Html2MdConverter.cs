using System.Xml.Linq;
using System.Xml.XPath;
using DevOps.ImmigrateTool.AtlassianConfluence.Entities.WikiPages;
using DevOps.ImmigrateTool.AtlassianConfluence.Utils;

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

        public bool StartConvertion()
        {
            ConfluencePageRef pageRef = ReadIndexOfPages();
            return true;
        }

        private ConfluencePageRef ReadIndexOfPages()
        {
            ConfluencePageRef page = null;
            
            string indexFilePath = System.IO.Path.Combine(_htmlSourceFolder, INDEX_FILE);
            
            XDocument xdoc = XDocument.Load(indexFilePath);
            
             XElement element = xdoc.XPathSelectElement("//div[@id='main-content']");

             if (element != null)
             {
                 foreach (XElement ele in element.Elements())
                 {
                     XAttribute attr = ele.Attribute("href");

                     if (attr != null)
                     {
                         string nodeVal = attr.Value;
                     }
                     
                 }
             }

             return page;
        }
    }
}