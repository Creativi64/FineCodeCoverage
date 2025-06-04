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
            _dateTimeService = dateTimeService;
            fileRenameListener.FileRenamedEvent += FileRenameListener_FileRenamedEvent;
        }

        private void FileRenameListener_FileRenamedEvent(IReadOnlyList<FileRename> fileRenames)
            => _reportFileLineCoverage?.FilesRenamed(fileRenames);

        public IFileLineCoverage Create(IReadOnlyList<IAssembly> assemblies)
        {
            _reportFileLineCoverage = new ReportFileLineCoverage(assemblies, _dateTimeService);
            return _reportFileLineCoverage;
        }
    }
}
