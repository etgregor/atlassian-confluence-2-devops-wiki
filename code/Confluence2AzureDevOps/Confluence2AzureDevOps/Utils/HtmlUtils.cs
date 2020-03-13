using System;
using System.Diagnostics;
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
                // string htmlContent =  IoUtils.ReadFileContent(filePath);
                //
                // if (string.IsNullOrEmpty(htmlContent))
                // {
                //     return false;
                // }
                //
                // htmlContent = RemoveMultiplesSpaces(htmlContent);
                    
                htmlDoc = new HtmlDocument {OptionFixNestedTags = true };
                
                //htmlDoc.LoadHtml(htmlContent);
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
                Trace.TraceWarning("TryReadDocumentAsHtml: {0}", e);
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
            catch (Exception e)
            {
                linkReference = null;
                Trace.TraceWarning("GetLinkInfo: {0}", e);
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
            catch (Exception e)
            {
                linkReference = null;
                Trace.TraceWarning("GetImgInfo: {0}", e);
            }

            return linkReference;
        }

        /// <summary>
        /// Get <see cref="HtmlNode"/> if node is Code Section
        /// </summary>
        /// <param name="htmlNode">Html node</param>
        /// <param name="codeSectionInfo">Code info, return if not is code section</param>
        /// <returns>True id is code section, false if not is</returns>
        internal static bool TryGetCodeSnipped(HtmlNode htmlNode, out CodeSectionInfo codeSectionInfo)
        {
            bool isCodeSection = false;

            codeSectionInfo = null;

            HtmlSectionType elementType = IdentifyElementType(htmlNode);

            try
            {
                if (elementType == HtmlSectionType.CodeSection)
                {
                    if (htmlNode.HasChildNodes)
                    {
                        string header = string.Empty;
                        string snippet = string.Empty;
                        string codeLanguage = string.Empty;

                        foreach (HtmlNode codeChild in htmlNode.ChildNodes)
                        {
                            string cssClassValue =
                                codeChild.GetAttributeValue(HtmlConstants.CSS_CLASS_ATTR, string.Empty);

                            if (cssClassValue.Contains(HtmlConstants.CSS_CLASS_FOR_CODE_HEADER))
                            {
                                header = codeChild.InnerText;
                            }
                            else if (cssClassValue.Contains(HtmlConstants.CSS_CLASS_FOR_CODE_BODY))
                            {
                                codeLanguage = GetCodeFlavor(codeChild, out snippet);
                            }
                            else if (cssClassValue.Contains(HtmlConstants.CSS_CLASS_FOR_CODE_CONTENT))
                            {
                                foreach (HtmlNode grandSon in codeChild.ChildNodes)
                                {
                                    string subChildClass =
                                        grandSon.GetAttributeValue(HtmlConstants.CSS_CLASS_ATTR, string.Empty);

                                    if (subChildClass.Contains(HtmlConstants.CSS_CLASS_FOR_CODE_BODY))
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
                                Title = header,
                                CodeSnippet = snippet,
                                Language = codeLanguage
                            };

                            isCodeSection = true;
                        }
                        else
                        {
                            Trace.TraceInformation("There is not code info");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                isCodeSection = false;
                Trace.TraceWarning("TryGetCodeSnipped: {0}", e);
            }

            return isCodeSection;
        }

        internal static bool TryGetMetadataInfo(HtmlNode htmlNode, out string metadata)
        {
            bool isMetadata = false;

            metadata = string.Empty;

            HtmlSectionType sectionType = IdentifyElementType(htmlNode);

            try
            {
                if (sectionType == HtmlSectionType.MetadataSection)
                {
                    string metadataValue = GetChildText(htmlNode);

                    if (!string.IsNullOrEmpty(metadataValue))
                    {
                        metadata = $"{HtmlConstants.ITALIC_STILE}{metadataValue.Trim()}{HtmlConstants.ITALIC_STILE}";
                    }

                    isMetadata = true;
                }
            }
            catch (Exception e)
            {
                isMetadata = false;
                Trace.TraceWarning("TryGetMetadataInfo: {0}", e);
            }

            return isMetadata;
        }


        internal static bool ContainsTableElement(HtmlNode htmlNode,out HtmlNode tableNode)
        {
            bool isTableElement = false;
            tableNode = null;
            
            HtmlSectionType sectionType = IdentifyElementType(htmlNode);

            try
            {
                if (sectionType == HtmlSectionType.TableSection)
                {
                    foreach (HtmlNode divChild in htmlNode.ChildNodes)
                    {
                        if (divChild.Name == HtmlConstants.HTML_TABLE)
                        {
                            tableNode = divChild;
                            isTableElement = true;
                            break;
                        }
                    }

                    if (!isTableElement)
                    {
                        Trace.TraceWarning("Element is table container but not contains element 'table'");
                    }
                }
            }
            catch (Exception e)
            {
                isTableElement = false;
                Trace.TraceWarning("ContainsTableElement: {0}", e);
            }

            return isTableElement;
        }

        private static HtmlSectionType IdentifyElementType(HtmlNode htmlNode)
        {
            HtmlSectionType type = HtmlSectionType.NotIdentified;

            try
            {
                string cssClassValue = htmlNode.GetAttributeValue(HtmlConstants.CSS_CLASS_ATTR, string.Empty);

                if (!string.IsNullOrEmpty(cssClassValue))
                {
                    if (cssClassValue.Contains(HtmlConstants.CSS_CLASS_FOR_CODE_SECTION))
                    {
                        type = HtmlSectionType.CodeSection;
                    }
                    else if (cssClassValue.Contains(HtmlConstants.CSS_CLASS_FOR_METADATA))
                    {
                        type = HtmlSectionType.MetadataSection;
                    }
                    else if (cssClassValue.Contains(HtmlConstants.CSS_CLASS_FOR_TABLE_CONTENT))
                    {
                        type = HtmlSectionType.TableSection;
                    }
                }
            }
            catch (Exception e)
            {
                type = HtmlSectionType.NotIdentified;
                Trace.TraceWarning("IdentifyElementType: {0}", e);
            }

            return type;
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

        internal static string RemoveMultiplesSpaces(string originalValue)
        {
            if (string.IsNullOrEmpty(originalValue))
            {
                return originalValue;
            }

            originalValue = Regex.Replace(originalValue, @"\s+", " ");

            return originalValue;
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

        private static string GetChildText(HtmlNode htmlNode)
        {
            var text = new StringBuilder();

            if (!string.IsNullOrEmpty(htmlNode.InnerText))
            {
                text.AppendFormat(htmlNode.InnerText.Trim(), ",");
            }

            if (htmlNode.HasChildNodes)
            {
                foreach (HtmlNode child in htmlNode.ChildNodes)
                {
                    text.Append(GetChildText(child));
                }
            }

            return text.ToString();
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
    }
}