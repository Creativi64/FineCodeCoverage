using System.ComponentModel.Composition;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(IDataCollectorSettingsBuilderFactory))]
    internal sealed class DataCollectorSettingsBuilderFactory : IDataCollectorSettingsBuilderFactory
    {
        public IDataCollectorSettingsBuilder Create() => new DataCollectorSettingsBuilder();
    }
}
