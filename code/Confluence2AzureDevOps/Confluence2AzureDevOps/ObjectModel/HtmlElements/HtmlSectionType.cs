namespace Confluence2AzureDevOps.ObjectModel.HtmlElements
{
    internal enum HtmlSectionType
    {
        NotIdentified,
        /// <summary>
        /// Is code section
        /// </summary>
        CodeSection,
        
        /// <summary>
        /// Is metadata section
        /// </summary>
        MetadataSection,
        
        /// <summary>
        /// Is table section
        /// </summary>
        TableSection
    }
}