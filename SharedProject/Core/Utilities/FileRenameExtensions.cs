using System;
using System.Collections.Generic;
using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
    internal static class FileRenameExtensions
    {
        public static IReadOnlyList<FileRename> TryUpdateDictionary<T>(this IEnumerable<FileRename> fileRenames, IDictionary<string, T> fileDictionary)
        {
            List<FileRename> renames = new List<FileRename>();
            foreach (FileRename fileRename in fileRenames)
            {
                if (fileDictionary != null && fileDictionary.TryGetValue(fileRename.OldFilePath, out T value))
                {
                    renames.Add(fileRename);
                    _ = fileDictionary.Remove(fileRename.OldFilePath);
                    fileDictionary.Add(fileRename.NewFilePath, value);
                }
            }
            return renames;
        }

        public static bool HasDirectoryChanged(this FileRename fileRename)
        {
            string oldDir = Path.GetDirectoryName(fileRename.OldFilePath);
            string newDir = Path.GetDirectoryName(fileRename.NewFilePath);
            return !string.Equals(oldDir, newDir, StringComparison.OrdinalIgnoreCase);
        }
    }
}
