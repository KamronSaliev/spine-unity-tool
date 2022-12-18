using System.IO;

namespace Editor.Utils
{
    public static class FileUtils
    {
        public static void CopyFiles(string fileExtension, string srcFolder, string destFolder, bool rewrite)
        {
            if (!Directory.Exists(srcFolder))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(srcFolder, "*." + fileExtension, SearchOption.AllDirectories))
            {
                var parentDirectory = Directory.GetParent(file);
                var modulePath = parentDirectory.FullName;
                var moduleName = parentDirectory.Name;

                var dir = Path.Combine(destFolder, moduleName);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                foreach (var subFolders in Directory.GetDirectories(modulePath, "*", SearchOption.AllDirectories))
                {
                    var dest = subFolders.Replace(modulePath, destFolder + Path.DirectorySeparatorChar + moduleName);

                    if (!Directory.Exists(dest))
                    {
                        Directory.CreateDirectory(dest);
                    }

                    CopyFiles(fileExtension, subFolders, dest, rewrite);
                }

                foreach (var allFiles in Directory.GetFiles(modulePath, "*." + fileExtension,
                             SearchOption.AllDirectories))
                {
                    if (File.Exists(allFiles) && !rewrite)
                    {
                        continue;
                    }

                    File.Copy(allFiles,
                        allFiles.Replace(modulePath, destFolder + Path.DirectorySeparatorChar + moduleName), rewrite);
                }
            }
        }
    }
}