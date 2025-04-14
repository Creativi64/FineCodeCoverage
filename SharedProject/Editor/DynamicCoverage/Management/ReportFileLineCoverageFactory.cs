using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [Export(typeof(IReportFileLineCoverageFactory))]
    internal class ReportFileLineCoverageFactory : IReportFileLineCoverageFactory
    {
        public IFileLineCoverage Create(IReadOnlyList<IAssembly> assemblies)
            => new ReportFileLineCoverage(assemblies);
    }
}
