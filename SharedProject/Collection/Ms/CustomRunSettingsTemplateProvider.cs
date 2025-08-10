using System.ComponentModel.Composition;
using System.IO;
using FineCodeCoverage.Utilities.Wrappers;

namespace FineCodeCoverage.Collection.Ms
{
    [Export(typeof(ICustomRunSettingsTemplateProvider))]
    internal sealed class CustomRunSettingsTemplateProvider : ICustomRunSettingsTemplateProvider
    {
        private const string TemplateName = "fcc-ms-runsettings-template.xml";
        private readonly IFileUtil _fileUtil;

        [ImportingConstructor]
        public CustomRunSettingsTemplateProvider(IFileUtil fileUtil) => _fileUtil = fileUtil;

        public CustomRunSettingsTemplateDetails Provide(string projectDirectory, string solutionDirectory)
        {
            CustomRunSettingsTemplateDetails runSettingsTemplate = GetTemplateIfExistsInDirectory(projectDirectory);
            return runSettingsTemplate ?? GetTemplateIfExistsInDirectory(solutionDirectory);
        }

        private CustomRunSettingsTemplateDetails GetTemplateIfExistsInDirectory(string directory)
        {
            if (directory == null)
            {
                return null;
            }

            string templatePath = Path.Combine(directory, TemplateName);
            return _fileUtil.Exists(templatePath)
                ? new CustomRunSettingsTemplateDetails
                {
                    Template = _fileUtil.ReadAllText(templatePath),
                    Path = templatePath,
                }
                : null;
        }
    }
}
