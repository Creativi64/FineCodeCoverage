using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    public static class IRunSettingsConfigurationInfoExtensions
    {
        public static bool IsTestExecution(this IRunSettingsConfigurationInfo configurationInfo)
            => configurationInfo.RequestState == RunSettingConfigurationInfoState.Execution;
    }
}