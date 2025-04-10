using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    public static class DirectoryResultsTreeBuilder
    {
        public static DirectoryNode BuildDirectoryTree(List<ISourceFile> codeFiles)
        {
            if (codeFiles == null || codeFiles.Count == 0)
                return null;

            // Determine the common root directory
            var rootPath = FindCommonRootPath(codeFiles.ConvertAll(cf => cf.Path));
            var root = new DirectoryNode(rootPath);

            // Add files to the tree
            foreach (var file in codeFiles)
            {
                var relativePath = FileUtil.GetRelativePath(rootPath, file.Path);
                AddFileToTree(root, relativePath, file);
            }

            return root;
        }

        public static IEnumerable<T> TakeAllButLast<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var enumerator = source.GetEnumerator();
            try
            {
                if (!enumerator.MoveNext())
                    yield break;

                var previous = enumerator.Current;

                while (enumerator.MoveNext())
                {
                    yield return previous;
                    previous = enumerator.Current;
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        private static string FindCommonRootPath(List<string> paths)
        {
            var splitPaths = paths.ConvertAll(p => p.Split(Path.DirectorySeparatorChar).TakeAllButLast().ToArray());
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
                if (!currentNode.SubDirectoryParts.ContainsKey(part))
                {
                    currentNode.SubDirectoryParts[part] = new DirectoryNode(part);
                }
                currentNode = currentNode.SubDirectoryParts[part];
            }

            // Add the file to the current directory
            currentNode.AddSourceFile(file);
        }

        public class DirectoryNode : IDirectory
        {
            public string Name { get; set; }
            public Dictionary<string, DirectoryNode> SubDirectoryParts { get; set; } = new Dictionary<string, DirectoryNode>();
            private List<ISourceFile> sourceFiles = new List<ISourceFile>();
            public IReadOnlyList<ISourceFile> SourceFiles => sourceFiles;
            public void AddSourceFile(ISourceFile sourceFile)
            {
                sourceFiles.Add(sourceFile);
            }
            public List<IDirectory> subDirectories;
            public IReadOnlyList<IDirectory> SubDirectories
            {
                get
                {
                    if(subDirectories == null)
                    {
                        subDirectories = SubDirectoryParts.Values.ToList<IDirectory>();
                    }
                    return subDirectories;
                }
            }

            public DirectoryNode(string name)
            {
                Name = name;
            }
        }
    }

}
