using System.IO;

namespace TuringSmartScreenTool.Entities
{
    public class LoadAccessory
    {
        private readonly DirectoryInfo _directory;

        public LoadAccessory(DirectoryInfo directory)
        {
            _directory = directory;
        }

        public string GetFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            var replacedFilePath = filePath.Replace('/', Path.DirectorySeparatorChar);
            var fileFullPath = _directory.FullName + replacedFilePath;
            if (string.IsNullOrEmpty(fileFullPath) || !File.Exists(fileFullPath))
                return null;

            return fileFullPath;
        }
    }
}
