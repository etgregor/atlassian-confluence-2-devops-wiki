using System;

namespace DevOps.ImmigrateTool.AtlassianConfluence.Base.CustomExceptions
{
    /// <summary>
    ///  Error al ejecutar el llamado a la API
    /// </summary>
    public class CallApiException : Exception
    {
        public CallApiException(string message) : base(message)
        {
        }

        public CallApiException(string message, Exception innerEx) : base(message, innerEx)
        {
        }
    }
}