using System.ComponentModel.Composition;
using System.IO;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    [Export(typeof(ICustomRunSettingsTemplateProvider))]
    internal class CustomRunSettingsTemplateProvider : ICustomRunSettingsTemplateProvider
    {
        private const string TemplateName = "fcc-ms-runsettings-template.xml";
        private readonly IFileUtil _fileUtil;

        [ImportingConstructor]
        public CustomRunSettingsTemplateProvider(IFileUtil fileUtil) => this._fileUtil = fileUtil;

        public CustomRunSettingsTemplateDetails Provide(string projectDirectory, string solutionDirectory)
        {
            CustomRunSettingsTemplateDetails runSettingsTemplate = this.GetTemplateIfExistsInDirectory(projectDirectory);
            return runSettingsTemplate ?? this.GetTemplateIfExistsInDirectory(solutionDirectory);
        }

        private CustomRunSettingsTemplateDetails GetTemplateIfExistsInDirectory(string directory)
        {
            if (directory == null)
            {
                return null;
            }

            string templatePath = Path.Combine(directory, TemplateName);
            return this._fileUtil.Exists(templatePath)
                ? new CustomRunSettingsTemplateDetails
                {
                    Template = this._fileUtil.ReadAllText(templatePath),
                    Path = templatePath
                }
                : null;
        }
    }
}