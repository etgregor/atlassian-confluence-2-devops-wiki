using HtmlAgilityPack;

namespace Confluence2AzureDevOps.ObjectModel.HtmlElements
{
    internal class CodeSectionInfo
    {
        private string _codeSnippet;
        
        private string _language;
        
        public string Title { get; set; }

        public string Language
        {
            get
            {
                return _language;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _language = value.Trim();    
                }
                else
                {
                    _language = string.Empty;
                }
            }
        }

        public string CodeSnippet
        {
            get => _codeSnippet;
            set => _codeSnippet = System.Web.HttpUtility.HtmlDecode(value);
        }

        public override string ToString()
        {
            string codeFormat = "{3}{4}{0}{4}{3}``` {1}{3}{2}{3}```{3}";

            return string.Format(codeFormat, Title, Language, _codeSnippet, HtmlConstants.NEW_LINE, HtmlConstants.BOLD_STILE);
        }
    }
}