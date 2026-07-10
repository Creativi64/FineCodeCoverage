using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Collection.Ms
{
    public static class IRunSettingsConfigurationInfoExtensions
    {
        public static bool IsTestExecution(this IRunSettingsConfigurationInfo configurationInfo)
            => configurationInfo.RequestState == RunSettingConfigurationInfoState.Execution;
    }
}
