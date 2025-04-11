using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [Export(typeof(IReportFileLineCoverageFactory))]
    internal class ReportFileLineCoverageFactory : IReportFileLineCoverageFactory
    {
        public IFileLineCoverage Create(Func<IDirectory> rootDirectoryProvider)
            => new ReportFileLineCoverage(rootDirectoryProvider);
    }
}
