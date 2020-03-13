using System;
using System.IO;
using System.Text;
using Confluence2AzureDevOps.Base.CustomExceptions;
using Newtonsoft.Json;

namespace Confluence2AzureDevOps.Utils
{
    internal static class IoUtils
    {
        public static void CreateFolderPath(string path, bool backupIfExists = true)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new GenericC2AException("Can't create empty directory");
            }
            
            if (Directory.Exists(path))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);

                if (dirInfo.Parent == null)
                {
                    throw new GenericC2AException($"Directory cant be root, please provide subdirectory path {path}");
                }

                System.Diagnostics.Trace.TraceInformation($"Create backup directory '{dirInfo.Name}'");
                
                string backupName = $"{dirInfo.Name}_Backup_{DateTime.Now:ddMMMyyHHmmss}";
                
                dirInfo.MoveTo(Path.Combine(dirInfo.Parent.FullName, backupName));
            }
            
            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Get full file path if file exists
        /// </summary>
        /// <param name="paths">Parts of path. i.e: ("root", "doc", "file1.txt")</param>
        /// <returns>Ful file path</returns>
        public static string GetPathIfFileExists(params string[] paths)
        {
            string filePath = System.IO.Path.Combine(paths);

            if (!System.IO.File.Exists(filePath))
            {
                filePath = string.Empty;
            }

            return filePath;
        }
        
        public static string ReadFileContent(params string[] paths)
        {
            string filePath = Path.Combine(paths);
            string fileContent = string.Empty;
            
            if (File.Exists(filePath))
            {
                string path = Path.Combine(filePath);
                fileContent = File.ReadAllText(path, Encoding.UTF8);
            }

            return fileContent;
        }
        
        public static void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new GenericC2AException("Can't create empty directory");
            }
            
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        
        public static void SaveFile(string filePath, string content)
        {
            Guard.PreventStringEmpty("filePath", filePath);

            using (FileStream fileStream = File.Create(filePath))
            {
                using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    writer.Write(content);
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