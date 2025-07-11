using System;
using System.Collections.Generic;
using FineCodeCoverage.VSAbstractions.Files;

namespace FineCodeCoverage.Collection.ReportGeneration
{
    public interface IDynamicReportResult : IReportResult
    {
        event EventHandler<IReadOnlyList<FileRename>> FileRenamedEvent;
    }
}
