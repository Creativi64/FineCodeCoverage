using System.Collections.Generic;
using System.IO;
using System.Linq;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Output
{
    public static class DirectoryResultsTreeBuilder
    {
        public static DirectoryNode BuildDirectoryTree(List<ISourceFile> codeFiles)
        {
            if (codeFiles == null || codeFiles.Count == 0)
                return null;

            // Determine the common root directory
            string rootPath = FindCommonRootPath(codeFiles.ConvertAll(cf => cf.Path));
            var root = new DirectoryNode(rootPath);

            // Add files to the tree
            foreach (ISourceFile file in codeFiles)
            {
                string relativePath = FileUtil.GetRelativePath(rootPath, file.Path);
                AddFileToTree(root, relativePath, file);
            }

            return root;
        }

        public static IEnumerable<T> TakeAllButLast<T>(this IEnumerable<T> source)
        {
            IEnumerator<T> enumerator = source.GetEnumerator();
            try
            {
                if (!enumerator.MoveNext())
                    yield break;

                T previous = enumerator.Current;

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
            List<string[]> splitPaths = paths.ConvertAll(p => p.Split(Path.DirectorySeparatorChar).TakeAllButLast().ToArray());
            string[] commonParts = splitPaths
                .Aggregate((current, next) => current.Zip(next, (c, n) => c == n ? c : null).TakeWhile(p => p != null).ToArray());
            return Path.Combine(commonParts);
        }

        private static void AddFileToTree(DirectoryNode currentNode, string relativePath, ISourceFile file)
        {
            string[] parts = relativePath.Split(Path.DirectorySeparatorChar);
            for (int i = 0; i < parts.Length - 1; i++) // Traverse directories
            {
                string part = parts[i];
                if (!currentNode.SubDirectoryParts.TryGetValue(part, out DirectoryNode value))
                {
                    value = new DirectoryNode(part);
                    currentNode.SubDirectoryParts[part] = value;
                }

                currentNode = value;
            }

            // Add the file to the current directory
            currentNode.AddSourceFile(file);
        }

        public class DirectoryNode : IDirectory
        {
            public string Name { get; set; }
            public Dictionary<string, DirectoryNode> SubDirectoryParts { get; set; } = new Dictionary<string, DirectoryNode>();
            private readonly List<ISourceFile> _sourceFiles = new List<ISourceFile>();
            public IReadOnlyList<ISourceFile> SourceFiles => this._sourceFiles;
            public void AddSourceFile(ISourceFile sourceFile) => this._sourceFiles.Add(sourceFile);
            private List<IDirectory> _subDirectories;
            public IReadOnlyList<IDirectory> SubDirectories
                => this._subDirectories ?? (this._subDirectories = this.SubDirectoryParts.Values.ToList<IDirectory>());

            public DirectoryNode(string name) => this.Name = name;
        }
    }
}
