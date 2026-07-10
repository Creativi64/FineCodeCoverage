using System;
using System.Collections.Generic;

namespace FineCodeCoverage.VSAbstractions.Files
{
    public interface IFileRenameListener
    {
        event Action<IReadOnlyList<FileRename>> FileRenamedEvent;
    }
}
