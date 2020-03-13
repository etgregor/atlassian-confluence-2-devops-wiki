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

        internal static bool TryGetCodeSnipped(HtmlNode htmlNode, out CodeSectionInfo codeSectionInfo)
        {
            bool isCodeSection = false;
            codeSectionInfo = null;
            
            try
            {
                string hrefValue = htmlNode.GetAttributeValue("class", string.Empty);

                if (!string.IsNullOrEmpty(hrefValue) && hrefValue.Contains("code panel"))
                {

                    if (htmlNode.HasChildNodes)
                    {
                        string header = string.Empty;
                        string snippet = string.Empty;
                        string codeLanguage = string.Empty;
                            
                        foreach (HtmlNode codeChild in htmlNode.ChildNodes)
                        {
                            string childClass = codeChild.GetAttributeValue("class", string.Empty);

                            if (childClass.Contains("codeHeader"))
                            {
                                header = codeChild.InnerText;
                            }
                            else if (childClass.Contains("syntaxhighlighter-pre"))
                            {
                                codeLanguage = GetCodeFlavor(codeChild, out snippet);
                            }
                            else if (childClass.Contains("codeContent"))
                            {
                                foreach (HtmlNode grandSon in codeChild.ChildNodes)
                                {
                                    string subChildClass = grandSon.GetAttributeValue("class", string.Empty);
                                    
                                    if (subChildClass.Contains("syntaxhighlighter-pre"))
                                    {
                                        codeLanguage = GetCodeFlavor(grandSon, out snippet);
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(snippet))
                        {
                            codeSectionInfo = new CodeSectionInfo
                            {
                                Header = header, 
                                CodeSnippet = snippet, 
                                Language = codeLanguage
                            };

                            isCodeSection = true;    
                        }
                        else
                        {
                            System.Diagnostics.Trace.TraceInformation("There is not code info");
                        }
                    }
                }
            }
            catch
            {
                isCodeSection = false;
            }

            return isCodeSection;
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

            return title;
        }

        private static string GetCodeFlavor(HtmlNode codeChild, out string codeSnippet)
        {
            string codeLanguage = string.Empty; 
            
            string syntaxHighlighter = codeChild.GetAttributeValue("data-syntaxhighlighter-params", string.Empty);

            codeSnippet = codeChild.InnerText;
            
            if (!string.IsNullOrEmpty(syntaxHighlighter))
            {
                var values = syntaxHighlighter.Split(';');

                var val = values.FirstOrDefault(p => p.StartsWith("brush"));

                if (!string.IsNullOrEmpty(val))
                {
                    var ops = val.Split(':');

                    if (ops.Length >= 1)
                    {
                        codeLanguage = ops[1];
                    }
                }
            }

            return codeLanguage;
        }
        
        
        private static string RemoveDiacritics(string text) 
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

        private  static string RemoveMultiplesSpaces(string originalValue)
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