using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Confluence2AzureDevOps.ObjectModel.HtmlElements;
using HtmlAgilityPack;

namespace Confluence2AzureDevOps.Utils
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
                        errors.AppendLine($"({error.Line}, {error.LinePosition}) => {error.Reason} ");
                    }

                    readError = errors.ToString();
                }

                readComplete = htmlDoc != null;
            }
            catch (Exception e)
            {
                readComplete = false;
                readError = e.Message;
            }

            return readComplete;
        }
        
        internal static LinkElementInfo GetLinkInfo(HtmlNode htmlNode)
        {
            LinkElementInfo linkReference = null;
            
            try
            {
                string hrefValue = htmlNode.GetAttributeValue("href", string.Empty);

                string linkCaption = RemoveMultiplesSpaces(htmlNode.InnerText);

                if (!string.IsNullOrEmpty(hrefValue))
                {
                    linkReference = new LinkElementInfo(hrefValue, linkCaption);
                }
            }
            catch
            {
                linkReference = null;
            }

            return linkReference;
        }
        
        internal static LinkElementInfo GetImgInfo(HtmlNode htmlNode)
        {
            LinkElementInfo linkReference = null;
            
            try
            {
                string hrefValue = htmlNode.GetAttributeValue("src", string.Empty);
                
                var uri = new Uri(hrefValue);
                
                string path = uri.GetLeftPart(UriPartial.Path);
                
                linkReference = new LinkElementInfo(path, string.Empty);
            }
            catch
            {
                linkReference = null;
            }

            return linkReference;
        }
        
        /// <summary>
        /// Remove invalid chars to create valid file name according to:
        /// <see cref="https://docs.microsoft.com/en-us/azure/devops/project/wiki/wiki-file-structure?view=azure-devops"/>
        /// </summary>
        /// <param name="title">original </param>
        /// <returns>Clean text</returns>
        internal static string ConvertToValidWikiFileName(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return title;
            }
            
            title = title.Replace("\n", " ");
            
            Regex diagonals = new Regex("[\\\\/']");
            title = diagonals.Replace(title, "-");
            
            Regex invalidChars = new Regex("[;\\\\/:*?\"<>|&']");
            
            title = invalidChars.Replace(title, " ");
            title = title.Replace(".", "_");
            
            title = RemoveMultiplesSpaces(title);

            title = RemoveDiacritics(title);

            //title = System.Web.HttpUtility.UrlEncode(title);
            
            return title;
        }
        
        static string RemoveDiacritics(string text) 
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        internal  static string RemoveMultiplesSpaces(string originalValue)
        {
            if (string.IsNullOrEmpty(originalValue))
            {
                return originalValue;
            }
            
            originalValue = Regex.Replace(originalValue, @"\s+", " ");

            return originalValue;
        }
    }
}