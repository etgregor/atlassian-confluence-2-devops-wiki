namespace Confluence2AzureDevOps.ObjectModel.HtmlElements
{
    internal class LinkElementInfo
    {
        public LinkElementInfo()
        {
        }

        public LinkElementInfo(string originalHref, string linkCaption)
        {
            OriginalRef = originalHref;
            Caption = linkCaption;

            ResourceType = ResourceType.Unknown;
            
            if (!string.IsNullOrEmpty(originalHref))
            {
                if (originalHref.StartsWith("attachments"))
                {
                    ResourceType = ResourceType.AttachmentLink;
                }
                else if (originalHref.StartsWith("http") || originalHref.StartsWith("https"))
                {
                    ResourceType = ResourceType.ExternalLink;
                }
                else
                {
                    ResourceType = ResourceType.PageExistsOnWiki;
                }
            }
        }
        
        /// <summary>
        /// 'href' attribute
        /// </summary>
        public string OriginalRef { get; set; }

        /// <summary>
        /// Value or caption of resource
        /// </summary>
        public string Caption { get; set; }
        
        /// <summary>
        /// Resource type, it depends on location
        /// </summary>
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// New reference location
        /// </summary>
        public string NewRef { get; set; }
    }
}