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
    }
}
