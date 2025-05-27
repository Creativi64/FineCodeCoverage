using System;
using System.Collections.Generic;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    interface IDynamicReportResult : IReportResult
    {
        event EventHandler<IReadOnlyList<FileRename>> FileRenamedEvent;
    }
}
