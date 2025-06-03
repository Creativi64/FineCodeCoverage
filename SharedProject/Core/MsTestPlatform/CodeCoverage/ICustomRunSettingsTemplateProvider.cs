namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal interface ICustomRunSettingsTemplateProvider
    {
        CustomRunSettingsTemplateDetails Provide(string projectDirectory, string solutionDirectory);
    }
}