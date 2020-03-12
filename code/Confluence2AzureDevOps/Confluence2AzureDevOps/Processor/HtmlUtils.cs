using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Confluence2AzureDevOps.Base.CustomExceptions;
using Confluence2AzureDevOps.ObjectModel;
using HtmlAgilityPack;

namespace Confluence2AzureDevOps.Processor
{
    internal static class HtmlUtils
    {
        public static bool TryReadDocumentAsHtml(string filePath, out HtmlDocument htmlDoc, out string readError)
        {
            bool readComplete;
            readError = string.Empty;
            htmlDoc = null;
            
            try
            {
                htmlDoc = new HtmlDocument {OptionFixNestedTags = true};
                htmlDoc.Load(filePath);

                if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
                {
                    var errors = new StringBuilder();

                    foreach (HtmlParseError error in htmlDoc.ParseErrors)
                    {
                        errors.AppendLine(error.Reason);
                    }

                    readComplete = errors.Length <= 0;
                }
                else
                {
                    readComplete = true;
                }
            }
            catch (Exception e)
            {
                readComplete = false;
                readError = e.Message;
            }

            return readComplete;
        }
        
        internal static LinkReference GetNodeInfo(HtmlNode htmlNode)
        {
            LinkReference linkReference = null;
            
            try
            {
                string hrefValue = htmlNode.GetAttributeValue("href", string.Empty);

                string pageTitle = htmlNode.InnerText;

                if (!string.IsNullOrEmpty(hrefValue))
                {
                    pageTitle = CleanupFileTitle(pageTitle);

                    if (string.IsNullOrEmpty(pageTitle))
                    {
                        throw new Exception($"Empty caption: {hrefValue}");
                    }

                    linkReference = new LinkReference(hrefValue, pageTitle);
                }
            }
            catch (Exception e)
            {
                //throw new Exception("Can't read attributes of <a> element", e);
                linkReference = null;
            }

            return linkReference;
        }
        
        /// <summary>
        /// Remove many empty spaces, and invalid chars for filename
        /// <see cref="https://docs.microsoft.com/en-us/azure/devops/project/wiki/wiki-file-structure?view=azure-devops"/>
        /// </summary>
        /// <param name="title">original </param>
        /// <returns>Clean text</returns>
        private static string CleanupFileTitle(string title)
        {
            title = title.Replace("\n", " ");
            title = title.Replace("/", "-");
            
            title = Regex.Replace(title, @"\s+", " ");
            
            return title;
        }
    }
}