namespace Confluence2AzureDevOps.ObjectModel.HtmlElements
{
    internal class LinkElementInfo
    {
        public LinkElementInfo()
        {
        }

        public LinkElementInfo(string originalHref, string linkCaption, bool isImage = false)
        {
            LinkHref = originalHref;
            LinkCaption = linkCaption;

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
        public string LinkHref { get; set; }

        /// <summary>
        /// Value or caption of resource
        /// </summary>
        public string LinkCaption { get; set; }
        
        public ResourceType ResourceType { get; set; }

        public string NewResourceLocation { get; set; }
    }
}