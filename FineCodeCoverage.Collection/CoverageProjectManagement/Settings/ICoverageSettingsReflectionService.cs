using System.Collections.Generic;
using System.Reflection;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    internal interface ICoverageSettingsReflectionService
    {
        List<PropertyInfo> CoverageSettingsPropertyInfos { get; }

        CoverageSettings CreateCoverageSettingsFromOptions(IEnumerable<object> coverageSettingsOptions);
    }
}
