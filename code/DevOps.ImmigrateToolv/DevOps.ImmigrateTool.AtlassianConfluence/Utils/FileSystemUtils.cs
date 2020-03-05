using DevOps.ImmigrateTool.AtlassianConfluence.Base.CustomExceptions;
using Newtonsoft.Json;

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
        
        public static void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new GenericException("Can't create empty directory");
            }
            
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        
        public static void SaveFile(string filePath, string content)
        {
            Guard.PreventStringEmpty("filePath", filePath);

            using (var logFile = System.IO.File.Create(filePath))
            {
                using (var logWriter = new System.IO.StreamWriter(logFile))
                {
                    logWriter.Write(content);
                }    
            }
        }

        public static void SaveObjectToFile(string filePath, object value)
        {
            var jsonText = JsonConvert.SerializeObject(value, Formatting.Indented);
            DeleteFile(filePath);
            SaveFile(filePath, jsonText);
        }
    }
}