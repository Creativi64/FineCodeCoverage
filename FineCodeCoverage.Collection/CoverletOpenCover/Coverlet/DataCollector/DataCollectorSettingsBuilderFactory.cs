using System.ComponentModel.Composition;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.DataCollector
{
    [Export(typeof(IDataCollectorSettingsBuilderFactory))]
    internal sealed class DataCollectorSettingsBuilderFactory : IDataCollectorSettingsBuilderFactory
    {
        public IDataCollectorSettingsBuilder Create() => new DataCollectorSettingsBuilder();
    }
}
