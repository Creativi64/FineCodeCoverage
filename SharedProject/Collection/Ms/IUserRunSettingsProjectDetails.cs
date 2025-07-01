using System.Collections.Generic;
using FineCodeCoverage.Collection.CoverageProjectManagement.ReferencedProjects;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal interface IUserRunSettingsProjectDetails
    {
        List<IReferencedProject> ExcludedReferencedProjects { get; set; }

        List<IReferencedProject> IncludedReferencedProjects { get; set; }

        string CoverageOutputFolder { get; set; }

        ICoverageSettings Settings { get; set; }

        string TestDllFile { get; set; }
    }
}
