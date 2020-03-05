using Newtonsoft.Json;

namespace Confluence2AzureDevOps.Base
{
    public class ApiErrorResult
    {
        [JsonProperty("errorCode")]
        public int ErrorCode
        {
            get;
            set;
        }
        
        [JsonProperty("eventId")]
        public int EventId
        {
            get;
            set;
        }
        
        [JsonProperty("message")]
        public string Message
        {
            get;
            set;
        }
        
        [JsonProperty("typeKey")]
        public string TypeKey
        {
            get;
            set;
        }
        
        [JsonProperty("typeName")]
        public string TypeName
        {
            get;
            set;
        }
    }
}