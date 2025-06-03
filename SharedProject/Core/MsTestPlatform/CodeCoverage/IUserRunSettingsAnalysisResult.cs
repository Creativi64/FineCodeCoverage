using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal interface IUserRunSettingsAnalysisResult
    {
        bool Suitable { get; }
        bool SpecifiedMsCodeCoverage { get; }
        List<ICoverageProject> ProjectsWithFCCMsTestAdapter { get; }
    }
}