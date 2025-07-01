using System.ComponentModel.Composition;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.DataCollector
{
    [Export(typeof(IRunSettingsCoverletConfigurationFactory))]
    internal sealed class RunSettingsCoverletConfigurationFactory : IRunSettingsCoverletConfigurationFactory
    {
        public IRunSettingsCoverletConfiguration Create() => new RunSettingsCoverletConfiguration();
    }
}
