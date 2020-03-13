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
                        // add move section: #### Foo
                        child.InnerHtml = $"{HtmlConstants.NEW_LINE} {HtmlConstants.INTERNAL_SECTION} See SubTable{_internalRefCount} {HtmlConstants.NEW_LINE}";
                        
                        //[Custom foo description](#foo)
                        cildContents.Append($"{HtmlConstants.NEW_LINE} {HtmlConstants.INTERNAL_LINK_TO_SECTION} SubTable{_internalRefCount} {HtmlConstants.NEW_LINE}");
                        cildContents.Append(tableNode.OuterHtml);
                        
                        _internalRefCount++;
                    }
                    else if (string.Equals(child.Name,  HtmlConstants.HTML_DIV) &&
                             HtmlUtils.TryGetCodeSnipped(child, out CodeSectionInfo codeSectionInfo))
                    {
                        // add move section: #### Foo
                        child.InnerHtml = $"{HtmlConstants.NEW_LINE} {HtmlConstants.INTERNAL_SECTION} See Code{_internalRefCount} {HtmlConstants.NEW_LINE}";
                        
                        //[Custom foo description](#foo)
                        cildContents.Append($"{HtmlConstants.NEW_LINE} {HtmlConstants.INTERNAL_LINK_TO_SECTION} Code{_internalRefCount} {HtmlConstants.NEW_LINE}");
                        cildContents.Append(codeSectionInfo);
                        
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