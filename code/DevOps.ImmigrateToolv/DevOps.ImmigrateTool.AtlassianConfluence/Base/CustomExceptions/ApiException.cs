using System;

namespace DevOps.ImmigrateTool.AtlassianConfluence.Base.CustomExceptions
{
    /// <summary>
    /// Error al llamar a la API.
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// Constructor general.
        /// </summary>
        /// <param name="message"></param>
        public ApiException(string message) : base(message)
        {
        }

        public ApiException(string message, ApiErrorResult details) : base(message)
        {
            this.Detail = details;
        }
 
        public ApiErrorResult Detail { get; private set; } 
    }
}