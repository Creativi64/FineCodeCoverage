using System.Collections.Generic;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.Ms
{
    internal interface IUserRunSettingsAnalysisResult
    {
        bool Suitable { get; }

        bool SpecifiedMsCodeCoverage { get; }

        List<ICoverageProject> ProjectsWithFCCMsTestAdapter { get; }
    }
}
