using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal interface IProjectRunSettingsFromTemplateResult
    {
        IExceptionReason ExceptionReason { get; }
        List<string> CustomTemplatePaths { get; }
        List<ICoverageProject> CoverageProjectsWithFCCMsTestAdapter { get; }
    }
}