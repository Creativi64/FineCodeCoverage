using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.Utilities
{
    interface IFileRenameListener
    {
        event Action<IReadOnlyList<FileRename>> FileRenamedEvent;
    }
}
