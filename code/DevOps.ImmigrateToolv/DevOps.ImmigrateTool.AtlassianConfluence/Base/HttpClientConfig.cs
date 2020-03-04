namespace DevOps.ImmigrateTool.AtlassianConfluence.Base
{
    public class HttpClientConfig
    {
        /// <summary>
        /// Request timeout
        /// </summary>
        public int? SecondsTimeout { get; set; }
        
        /// <summary>
        /// Authorization type
        /// </summary>
        public string AuthorizationType { get; set; }
        
        /// <summary>
        /// Authorization header value
        /// </summary>
        public string AuthorizationValue { get; set; }
    }
}