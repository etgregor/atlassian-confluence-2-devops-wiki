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

        /// <summary>
        /// Element that contains child <see cref="CSS_CLASS_FOR_CODE_BODY"/>
        /// </summary>
        internal const string CSS_CLASS_FOR_CODE_CONTENT = "codeContent";
        
        /// <summary>
        /// Element that contains the code
        /// </summary>
        internal const string CSS_CLASS_FOR_CODE_BODY = "syntaxhighlighter-pre";

        #endregion

        /// <summary>
        /// Class name for div that has metadata info
        /// </summary>
        internal const string CSS_CLASS_FOR_METADATA = "page-metadata";

        internal const string CSS_CLASS_FOR_TABLE_CONTENT = "table-wrap";
        
        #region - HTML item names -

        /// <summary>
        /// Name of DIV control element
        /// </summary>
        internal const string HTML_DIV = "div";
        
        /// <summary>
        /// Name of SCRIPT control element
        /// </summary>
        internal const string HTML_SCRIPT = "scrtip";
        
        /// <summary>
        /// Name of HEAD control element
        /// </summary>
        internal const string HTML_HEAD = "head";
        
        /// <summary>
        /// Name of TABLE control element
        /// </summary>
        internal const string HTML_TABLE = "table";

        #endregion
        
        /// <summary>
        /// Replaceable tag by:  #, it will looks like: [Custom foo description](#foo)
        /// </summary>
        internal const string INTERNAL_LINK_TO_SECTION = "GGGINTERNLINKTOSECTIONGGG";
        
        /// <summary>
        /// Replaceable tag by tag:  ####, it will looks like: #### Foo
        /// </summary>
        internal const string INTERNAL_SECTION = "GGGINTERNSECTIONGGG";
    }
}