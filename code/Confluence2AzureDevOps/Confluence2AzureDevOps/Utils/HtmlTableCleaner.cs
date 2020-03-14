using System.Text;
using Confluence2AzureDevOps.ObjectModel.HtmlElements;
using HtmlAgilityPack;

namespace Confluence2AzureDevOps.Utils
{
    internal class HtmlTableCleaner
    {
        private HtmlNode _htmlItem;

        private StringBuilder cildContents;

        private int _internalRefCount = 0;
        
        public HtmlTableCleaner(HtmlNode htmlItem)
        {
            _htmlItem = htmlItem;
            
            cildContents = new StringBuilder();
        }

        public string GetTableDefinition()
        {
            SplitSubChild(_htmlItem);

            string result = $"{_htmlItem.OuterHtml} {cildContents}";

            return result;
        }
        
        private void SplitSubChild(HtmlNode htmlNode)
        {
            if (htmlNode.HasChildNodes)
            {
                foreach (HtmlNode child in htmlNode.ChildNodes)
                {
                    if (string.Equals(child.Name, HtmlConstants.HTML_DIV) 
                             && HtmlUtils.ContainsTableElement(child, out HtmlNode tableNode))
                    {
                        //[Custom foo description](#foo)
                        cildContents.Append($"{HtmlConstants.NEW_LINE} #### SubTable{_internalRefCount} {HtmlConstants.NEW_LINE}");
                        cildContents.Append(tableNode.OuterHtml);
                        cildContents.Append(HtmlConstants.NEW_LINE);
                        
                        // add move section: #### Foo
                        tableNode.InnerHtml = $" [See SubTable{_internalRefCount}](#SubTable{_internalRefCount}) ";
                        
                        _internalRefCount++;
                    }
                    else if (string.Equals(child.Name,  HtmlConstants.HTML_DIV) &&
                             HtmlUtils.TryGetCodeSnipped(child, out CodeSectionInfo codeSectionInfo))
                    {
                        //[Custom foo description](#foo)
                        cildContents.Append($"{HtmlConstants.NEW_LINE} #### Code{_internalRefCount} {HtmlConstants.NEW_LINE}");
                        cildContents.Append(codeSectionInfo);
                        cildContents.Append(HtmlConstants.NEW_LINE);
                        
                        // add move section: #### Foo
                        child.InnerHtml = $"[See Code{_internalRefCount}](#Code{_internalRefCount})";
                        
                        _internalRefCount++;    
                    }
                    else if (child.HasChildNodes)
                    {
                        SplitSubChild(child);    
                    }
                }
            }
        }
    }
}