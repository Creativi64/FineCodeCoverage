namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal static class UserRunSettingsAnalysisResultExtensions
    {
        public static bool Ok(this IUserRunSettingsAnalysisResult userRunSettingsAnalysisResult)
        {
            if (userRunSettingsAnalysisResult == null)
            {
                return false;
            }
            return userRunSettingsAnalysisResult.Suitable;

        }
    }
}
