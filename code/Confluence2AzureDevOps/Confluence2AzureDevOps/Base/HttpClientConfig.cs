namespace Confluence2AzureDevOps.Base
{
    /// <summary>
    /// Http client configuration
    /// </summary>
    public class HttpClientConfig
    {
        /// <summary>
        /// Request timeout
        /// </summary>
        public int? SecondsTimeout { get; set; }
        
        /// <summary>
        /// Authentication scheme <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Authentication"/>
        /// </summary>
        public string AuthorizationType { get; set; }
        
        /// <summary>
        /// value of "Autorization" header, exclude <see cref="AuthorizationType"/> value.
        /// </summary>
        public string AuthorizationValue { get; set; }
    }
}