namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal static class UserRunSettingsAnalysisResultExtensions
    {
        public static bool Ok(this IUserRunSettingsAnalysisResult userRunSettingsAnalysisResult)
            => userRunSettingsAnalysisResult?.Suitable == true;
    }
}
