namespace FineCodeCoverage.Collection.Ms
{
    internal static class UserRunSettingsAnalysisResultExtensions
    {
        public static bool Ok(this IUserRunSettingsAnalysisResult userRunSettingsAnalysisResult)
            => userRunSettingsAnalysisResult?.Suitable == true;
    }
}
