using FineCodeCoverage.Engine.ReportGenerator;
using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    public interface ISourceFile
    {
        event EventHandler HasNewCodeChanged;
        event EventHandler PathChanged;
        string Path { get; }
        IReadOnlyList<IClass> Classes { get; }
        bool HasNewCode { get; }
    }
}
