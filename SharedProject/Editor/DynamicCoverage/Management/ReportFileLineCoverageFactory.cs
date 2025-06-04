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
        private readonly IDateTimeService _dateTimeService;
        private ReportFileLineCoverage _reportFileLineCoverage;

        [ImportingConstructor]
        public ReportFileLineCoverageFactory(IDateTimeService dateTimeService, IFileRenameListener fileRenameListener)
        {
            this._dateTimeService = dateTimeService;
            fileRenameListener.FileRenamedEvent += this.FileRenameListener_FileRenamedEvent;
        }

        private void FileRenameListener_FileRenamedEvent(IReadOnlyList<FileRename> fileRenames)
            => this._reportFileLineCoverage?.FilesRenamed(fileRenames);

        public IFileLineCoverage Create(IReadOnlyList<IAssembly> assemblies)
        {
            this._reportFileLineCoverage = new ReportFileLineCoverage(assemblies, this._dateTimeService);
            return this._reportFileLineCoverage;
        }
    }
}
