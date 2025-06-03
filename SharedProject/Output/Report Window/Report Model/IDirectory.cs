using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    public interface IDirectory
    {
        string Name { get; }
        IReadOnlyList<IDirectory> SubDirectories { get; }
        IReadOnlyList<ISourceFile> SourceFiles { get; }
    }
}