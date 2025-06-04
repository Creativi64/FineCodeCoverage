using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Coverlet;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(IRunSettingsCoverletConfigurationFactory))]
    internal class RunSettingsCoverletConfigurationFactory : IRunSettingsCoverletConfigurationFactory
    {
        public IRunSettingsCoverletConfiguration Create() => new RunSettingsCoverletConfiguration();
    }
}
