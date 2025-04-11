using System;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IReportFileLineCoverageFactory
    {
        IFileLineCoverage Create(Func<IDirectory> rootDirectoryProvider);
    }
}
