using System.Collections.Generic;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.Ms
{
    internal interface IProjectRunSettingsFromTemplateResult
    {
        IExceptionReason ExceptionReason { get; }

        List<string> CustomTemplatePaths { get; }

        List<ICoverageProject> CoverageProjectsWithFCCMsTestAdapter { get; }
    }
}
