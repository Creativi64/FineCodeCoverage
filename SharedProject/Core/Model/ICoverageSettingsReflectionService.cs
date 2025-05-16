using FineCodeCoverage.Options;
using System.Collections.Generic;
using System.Reflection;

namespace FineCodeCoverage.Engine.Model
{
    interface ICoverageSettingsReflectionService
    {
        List<PropertyInfo> CoverageSettingsPropertyInfos { get; }
        CoverageSettings CreateCoverageSettingsFromOptions(IEnumerable<object> coverageSettingsOptions);
    }
}
