using System;

namespace Confluence2AzureDevOps.Base.CustomExceptions
{
    /// <summary>
    /// Processing exception
    /// </summary>
    public class GenericC2AException : InvalidOperationException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public GenericC2AException(string message) : base(message)
        {
        }

        public GenericC2AException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}