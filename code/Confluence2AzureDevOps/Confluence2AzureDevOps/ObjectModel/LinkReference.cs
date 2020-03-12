namespace Confluence2AzureDevOps.ObjectModel
{
    public enum LinkResourceType
    {
        Unknown = 0,
        ResourceOnSite,
        Attachment,
        ExternalLink
    }
    
    public class LinkReference
    {
        public LinkReference()
        {
        }

        public LinkReference(string originalHref, string linkCaption)
        {
            LinkHref = originalHref;
            LinkCaption = linkCaption;

            this.ResourceType = LinkResourceType.Unknown;

            this.NewResourceLocation = originalHref;
            
            if (!string.IsNullOrEmpty(originalHref))
            {
                if (originalHref.StartsWith("attachments"))
                {
                    ResourceType = LinkResourceType.Attachment;
                }
                else if (originalHref.StartsWith("http") || originalHref.StartsWith("https"))
                {
                    ResourceType = LinkResourceType.ExternalLink;
                }
                else
                {
                    ResourceType = LinkResourceType.ResourceOnSite;
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
        
        public LinkResourceType ResourceType { get; set; }

        public string NewResourceLocation { get; set; }
    }
}