using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.Utilities
{
    internal class FileRename
    {
        public FileRename(string oldFilePath, string newFilePath)
        {
            OldFilePath = oldFilePath;
            NewFilePath = newFilePath;
        }

        public string OldFilePath { get; }
        public string NewFilePath { get; }
    }
    interface IFileRenameListener
    {
        event Action<List<FileRename>> FileRenamedEvent;
    }
}
