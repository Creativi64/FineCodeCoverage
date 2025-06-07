using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IFileRenameListener
    {
        event Action<IReadOnlyList<FileRename>> FileRenamedEvent;
    }
}
