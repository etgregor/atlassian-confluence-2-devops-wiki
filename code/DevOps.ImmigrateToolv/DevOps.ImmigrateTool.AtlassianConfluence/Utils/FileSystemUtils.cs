using System;
using DevOps.ImmigrateTool.AtlassianConfluence.Base.CustomExceptions;

namespace DevOps.ImmigrateTool.AtlassianConfluence.Utils
{
    internal static class IoUtils
    {
        public static void CreateFolderPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new GenericException("Can't create empty directory");
            }
            
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }
    }
}