using System.Collections.Generic;

namespace FineCodeCoverage.Engine.Model
{
    internal interface ICoverageSettingsOptionsProvider
    {
        IEnumerable<object> Get();
    }
}
