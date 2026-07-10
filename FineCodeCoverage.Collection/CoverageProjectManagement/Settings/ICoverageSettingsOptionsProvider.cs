using System.Collections.Generic;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    internal interface ICoverageSettingsOptionsProvider
    {
        IEnumerable<object> Get();
    }
}
