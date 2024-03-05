using System;

namespace Confluence2AzureDevOps.Base.CustomExceptions
{
    /// <summary>
    ///  Error de acceso a la API
    /// </summary>
    public class UnautorizeApiException : Exception
    {
        public UnautorizeApiException(string message) : base(message){
            
        }
    }
}