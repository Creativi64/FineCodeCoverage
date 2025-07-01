using System;
using System.Collections.Generic;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    public interface IDynamicReportResult : IReportResult
    {
        event EventHandler<IReadOnlyList<FileRename>> FileRenamedEvent;
    }
}
