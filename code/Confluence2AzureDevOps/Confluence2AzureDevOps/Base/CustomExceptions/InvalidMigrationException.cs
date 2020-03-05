using System;

namespace Confluence2AzureDevOps.Base.CustomExceptions
{
    /// <summary>
    /// Processing exception
    /// </summary>
    public class GenericException : InvalidOperationException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public GenericException(string message) : base(message)
        {
        }

        public GenericException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}