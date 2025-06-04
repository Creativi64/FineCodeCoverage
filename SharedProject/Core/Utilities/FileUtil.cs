using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IFileUtil))]
    internal class FileUtil : IFileUtil
    {
        public string CreateTempDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _ = Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public void TryDeleteDirectory(string directory) => new DirectoryInfo(directory).TryDelete();

        public bool DirectoryExists(string directory) => Directory.Exists(directory);

        public string EnsureAbsolute(string directory, string possiblyRelativeTo)
        {
            if (!Path.IsPathRooted(directory))
            {
                directory = Path.GetFullPath(Path.Combine(possiblyRelativeTo, directory));
            }

            return directory;
        }

        public string FileDirectoryPath(string filePath) => new FileInfo(filePath).Directory.FullName;

        public string ReadAllText(string path) => File.ReadAllText(path);

        public void TryEmptyDirectory(string directory)
        {
            var directoryInfo = new DirectoryInfo(directory);
            if (!directoryInfo.Exists)
            {
                return;
            }

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.TryDelete();
            }

            foreach (DirectoryInfo subDir in directoryInfo.GetDirectories())
            {
                subDir.TryDelete(true);
            }
        }

        public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents);

        public bool Exists(string filePath) => File.Exists(filePath);

        public void Copy(string source, string destination) => File.Copy(source, destination);

        public string DirectoryParentPath(string directoryPath)
        {
            DirectoryInfo parentDirectory = new DirectoryInfo(directoryPath).Parent;
            return parentDirectory?.FullName;
        }

        public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
            => Directory.GetFiles(path, searchPattern, searchOption);

        public void DeleteFile(string filePath) => File.Delete(filePath);

        public static string GetRelativePath(string basePath, string fullPath)
        {
            var baseUri = new Uri(AppendDirectorySeparator(basePath));
            var fullUri = new Uri(fullPath);
            return Uri.UnescapeDataString(baseUri.MakeRelativeUri(fullUri).ToString())
                .Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendDirectorySeparator(string path)
        {
            // Ensure the path starts with a drive letter and a backslash
            if (path.Length > 1 && path[1] == ':' && path[2] != '\\')
            {
                path = path.Insert(2, "\\");
            }
            // Ensure the directory path ends with a separator
            return path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path : path + Path.DirectorySeparatorChar;
        }
    }
}
