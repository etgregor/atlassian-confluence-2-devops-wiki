namespace Confluence2AzureDevOps.ObjectModel.HtmlElements
{
    public static class HtmlConstants
    {
        #region - TEMP TAGS -

        /// <summary>
        /// Temporal id for add new line
        /// </summary>
        internal const string NEW_LINE = "ADDNEWLINE";
        
        /// <summary>
        /// Temporal id for add bold style
        /// </summary>
        internal const string BOLD_STILE = "ADDBOLDSTYLE";
        
        /// <summary>
        /// Temporal id for add italic style
        /// </summary>
        internal const string ITALIC_STILE = "ADDITALICSTYLE";

        #endregion

        /// <summary>
        /// Attribute for identify css class 
        /// </summary>
        internal const string CSS_CLASS_ATTR = "class";

        #region - Code -

        /// <summary>
        /// css class name for identify code section
        /// </summary>
        internal const string CSS_CLASS_FOR_CODE_SECTION = "code panel";
        
        /// <summary>
        /// css class name for identify header of code section 
        /// </summary>
        internal const string CSS_CLASS_FOR_CODE_HEADER = "codeHeader";

        internal const string CSS_CLASS_FOR_CODE_CONTENT = "codeContent";
        
        internal const string CSS_CLASS_FOR_CODE_BODY = "syntaxhighlighter-pre";

        #endregion

        internal const string CSS_CLASS_FOR_METADATA = "page-metadata";
    }
}