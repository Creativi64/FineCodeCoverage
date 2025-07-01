using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Coverlet;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(IRunSettingsCoverletConfigurationFactory))]
    internal sealed class RunSettingsCoverletConfigurationFactory : IRunSettingsCoverletConfigurationFactory
    {
        public IRunSettingsCoverletConfiguration Create() => new RunSettingsCoverletConfiguration();
    }
}
