namespace Confluence2AzureDevOps.ObjectModel.HtmlElements
{
    public class CodeSectionInfo
    {
        private string _codeSnippet;
        public string Header { get; set; }

        public string Language { get; set; }

        public string CodeSnippet
        {
            get { return _codeSnippet;}
            set { _codeSnippet = System.Web.HttpUtility.HtmlDecode(value);
        } }

        public override string ToString()
        {
            string codeFormat = "\n  {0} \n\n ``` {1} \n {2} \n ``` ";

            return string.Format(codeFormat, Header, Language, _codeSnippet);
        }
    }
}