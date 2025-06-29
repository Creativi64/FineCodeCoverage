using System;
using System.Collections.Generic;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal interface IDynamicReportResult : IReportResult
    {
        event EventHandler<IReadOnlyList<FileRename>> FileRenamedEvent;
    }
}
