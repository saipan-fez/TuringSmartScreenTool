using System.IO;

namespace TuringSmartScreenTool.Helpers
{
    public static class DirectoryInfoHelper
    {
        public static DirectoryInfo GetTempDirectory()
        {
            string tempDirPath;
            do
            {
                tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            }
            while (Directory.Exists(tempDirPath));

            var tempDirInfo = new DirectoryInfo(tempDirPath);
            tempDirInfo.Create();
            return tempDirInfo;
        }

        public static void DeleteRecursive(this DirectoryInfo directoryInfo)
        {
            if (!directoryInfo.Exists)
                return;

            foreach (var dir in directoryInfo.EnumerateDirectories())
            {
                DeleteRecursive(dir);
            }
            var files = directoryInfo.GetFiles();
            foreach (var file in files)
            {
                file.IsReadOnly = false;
                file.Delete();
            }
            directoryInfo.Delete();
        }

        public static void CopyRecursive(this DirectoryInfo sourceDir, DirectoryInfo destinationDir, bool recursive)
        {
            if (!sourceDir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir.FullName}");

            var srcSubDirectories = sourceDir.GetDirectories();

            if (!destinationDir.Exists)
            {
                destinationDir.Create();
            }

            foreach (var file in sourceDir.GetFiles())
            {
                var filePath = Path.Combine(destinationDir.FullName, file.Name);
                file.CopyTo(filePath);
            }

            if (recursive)
            {
                foreach (var srcSubDir in srcSubDirectories)
                {
                    var destSubDir = Path.Combine(destinationDir.FullName, srcSubDir.Name);
                    srcSubDir.CopyRecursive(new DirectoryInfo(destSubDir), true);
                }
            }
        }
    }
}
