using Confluence2AzureDevOps.Base.CustomExceptions;

namespace Confluence2AzureDevOps.Utils
{
    internal static class Guard
    {
        public static void PreventStringEmpty(string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new GenericException($" Require value for: {name}");
            }
        }

        public static void PreventDirectoryNotExistt(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                throw new GenericException($" Directory not exists: {path}");
            }
        }
    }
    
}