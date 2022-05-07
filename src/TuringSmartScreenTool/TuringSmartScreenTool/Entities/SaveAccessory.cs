using System;
using System.IO;

namespace TuringSmartScreenTool.Entities
{
    public class SaveAccessory
    {
        private readonly DirectoryInfo _assetsDirectory;

        public SaveAccessory(DirectoryInfo assetsDirectory)
        {
            _assetsDirectory = assetsDirectory;
        }

        public string SaveAssetFile(string srcFilePath)
        {
            if (string.IsNullOrEmpty(srcFilePath) || !File.Exists(srcFilePath))
                return null;

            var destFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(srcFilePath);
            var destFileFullPath = Path.Combine(_assetsDirectory.FullName, destFileName);

            File.Copy(srcFilePath, destFileFullPath);

            return $"/{_assetsDirectory.Name}/{destFileName}";
        }
    }
}
