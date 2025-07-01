using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.Utilities
{
    public interface IFileRenameListener
    {
        event Action<IReadOnlyList<FileRename>> FileRenamedEvent;
    }
}
