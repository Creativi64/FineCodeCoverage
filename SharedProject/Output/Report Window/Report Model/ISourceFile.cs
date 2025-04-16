using FineCodeCoverage.Engine.ReportGenerator;
using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    public interface ISourceFile
    {
        string Path { get; }
        IReadOnlyList<IClass> Classes { get; }
    }
}
