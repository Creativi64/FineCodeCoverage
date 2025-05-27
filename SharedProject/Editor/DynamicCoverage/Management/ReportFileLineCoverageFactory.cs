using System.Collections.Generic;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage.Utilities;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [Export(typeof(IReportFileLineCoverageFactory))]
    internal class ReportFileLineCoverageFactory : IReportFileLineCoverageFactory
    {
        private readonly IDateTimeService dateTimeService;
        private ReportFileLineCoverage reportFileLineCoverage;

        [ImportingConstructor]
        public ReportFileLineCoverageFactory(IDateTimeService dateTimeService, IFileRenameListener fileRenameListener)
        {
            this.dateTimeService = dateTimeService;
            fileRenameListener.FileRenamedEvent += this.FileRenameListener_FileRenamedEvent;
        }

        private void FileRenameListener_FileRenamedEvent(IReadOnlyList<FileRename> fileRenames)
            => this.reportFileLineCoverage?.FilesRenamed(fileRenames);

        public IFileLineCoverage Create(IReadOnlyList<IAssembly> assemblies)
        {
            this.reportFileLineCoverage = new ReportFileLineCoverage(assemblies, this.dateTimeService);
            return this.reportFileLineCoverage;
        }
    }
}
