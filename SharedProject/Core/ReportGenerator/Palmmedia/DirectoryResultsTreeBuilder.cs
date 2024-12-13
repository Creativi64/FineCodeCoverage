using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    public static class DirectoryResultsTreeBuilder
    {
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
        public static DirectoryNode BuildDirectoryTree(List<ISourceFile> codeFiles)
        {
            if (codeFiles == null || codeFiles.Count == 0)
                throw new ArgumentException("No files provided.");

            // Determine the common root directory
            var rootPath = FindCommonRootPath(codeFiles.Select(cf => cf.Path).ToList());
            var root = new DirectoryNode(rootPath);

            // Add files to the tree
            foreach (var file in codeFiles)
            {
                var relativePath = GetRelativePath(rootPath, file.Path);
                AddFileToTree(root, relativePath, file);
            }

            return root;
        }

        private static string FindCommonRootPath(List<string> paths)
        {
            var splitPaths = paths.Select(p => p.Split(Path.DirectorySeparatorChar)).ToList();
            var commonParts = splitPaths
                .Aggregate((current, next) => current.Zip(next, (c, n) => c == n ? c : null).TakeWhile(p => p != null).ToArray());
            return Path.Combine(commonParts);
        }

        private static void AddFileToTree(DirectoryNode currentNode, string relativePath, ISourceFile file)
        {
            var parts = relativePath.Split(Path.DirectorySeparatorChar);
            for (int i = 0; i < parts.Length - 1; i++) // Traverse directories
            {
                var part = parts[i];
                if (!currentNode.SubDirectories.ContainsKey(part))
                {
                    currentNode.SubDirectories[part] = new DirectoryNode(part);
                }
                currentNode = currentNode.SubDirectories[part];
            }

            // Add the file to the current directory
            currentNode.SourceFiles.Add(file);
        }

        public class DirectoryNode : IDirectory
        {
            public string Name { get; set; }
            public Dictionary<string, DirectoryNode> SubDirectories { get; set; } = new Dictionary<string, DirectoryNode>();
            public List<ISourceFile> SourceFiles { get; set; } = new List<ISourceFile>();
            public List<IDirectory> Children => SubDirectories.Values.ToList<IDirectory>();

            public DirectoryNode(string name)
            {
                Name = name;
            }
        }
    }

}
