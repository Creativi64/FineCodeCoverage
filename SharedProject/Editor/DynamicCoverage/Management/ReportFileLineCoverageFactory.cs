using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using FineCodeCoverage.Editor.DynamicCoverage.Utilities;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [Export(typeof(IReportFileLineCoverageFactory))]
    internal class ReportFileLineCoverageFactory : IReportFileLineCoverageFactory
    {
        private readonly IDateTimeService dateTimeService;

        [ImportingConstructor]
        public ReportFileLineCoverageFactory(IDateTimeService dateTimeService) => this.dateTimeService = dateTimeService;
        public IFileLineCoverage Create(IReadOnlyList<IAssembly> assemblies)
            => new ReportFileLineCoverage(assemblies, this.dateTimeService);
    }
}
