using System.Collections.Generic;
using System.Reflection;

namespace FineCodeCoverage.Engine.Model
{
    internal interface ICoverageSettingsReflectionService
    {
        List<PropertyInfo> CoverageSettingsPropertyInfos { get; }
        CoverageSettings CreateCoverageSettingsFromOptions(IEnumerable<object> coverageSettingsOptions);
    }
}
