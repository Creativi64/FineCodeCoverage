namespace FineCodeCoverage.Collection.Ms
{
    internal interface ICustomRunSettingsTemplateProvider
    {
        CustomRunSettingsTemplateDetails Provide(string projectDirectory, string solutionDirectory);
    }
}
