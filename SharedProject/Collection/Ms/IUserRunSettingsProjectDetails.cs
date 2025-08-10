using System.Collections.Generic;
using FineCodeCoverage.Collection.CoverageProjectManagement.ReferencedProjects;
using FineCodeCoverage.Collection.CoverageProjectManagement.Settings;

namespace FineCodeCoverage.Collection.Ms
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
