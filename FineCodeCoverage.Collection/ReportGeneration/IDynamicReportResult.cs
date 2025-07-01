using System;
using System.Collections.Generic;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Collection.ReportGeneration
{
    public interface IDynamicReportResult : IReportResult
    {
        event EventHandler<IReadOnlyList<FileRename>> FileRenamedEvent;
    }
}
