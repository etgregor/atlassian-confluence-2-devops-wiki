using System;

namespace Confluence2AzureDevOps.Base.CustomExceptions
{
    public class ApiInvalidInputDataException : Exception
    {
        public ApiInvalidInputDataException(string message, ApiErrorResult details) : base(message)
        {
            this.Detail = details;
        }

        public ApiErrorResult Detail { get; private set; } 
    }
}