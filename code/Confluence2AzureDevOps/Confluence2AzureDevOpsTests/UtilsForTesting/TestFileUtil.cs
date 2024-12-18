using System.IO;

namespace Confluence2AzureDevOpsTests.UtilsForTesting
{
    public static class TestFileUtil
    {
        private const string OUTPUT_DIRECTORY = "bin";
        
        public static string ReadContentFromResource(params string[] fileName)
        {   
            string path = Path.Combine(fileName);
            var fileContent = File.ReadAllText(path);
            return fileContent;
        }
        
        public static byte[] ReadFile(params string[] fileName)
        {   
            string path = Path.Combine(fileName);
            var fileContent = File.ReadAllBytes(path);
            return fileContent;
        }
        
        public static string WriteFile(string fileName, string fileContent)
        {
            string filename = Path.Combine("unit_test_output", fileName);

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            else
            {
                CreatePath(filename);
            }

            using (var writer = System.IO.File.CreateText(filename))
            {
                writer.WriteLine(fileContent); //or .Write(), if you wish
            }

            return filename;
        }

        private static void CreatePath(string file)
        {
            var fullpath = new FileInfo(file).Directory.FullName;

            if (!Directory.Exists(fullpath))
            {
                Directory.CreateDirectory(fullpath);
            }
        }
    }
}