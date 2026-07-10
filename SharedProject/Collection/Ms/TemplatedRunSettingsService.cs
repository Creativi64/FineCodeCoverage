using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.Ms
{
    [Export(typeof(ITemplatedRunSettingsService))]
    internal sealed class TemplatedRunSettingsService : ITemplatedRunSettingsService
    {
        private readonly IRunSettingsTemplate _runSettingsTemplate;
        private readonly ICustomRunSettingsTemplateProvider _customRunSettingsTemplateProvider;
        private readonly IRunSettingsTemplateReplacementsFactory _runSettingsTemplateReplacementsFactory;
        private readonly IProjectRunSettingsGenerator _projectRunSettingsGenerator;

        [ImportingConstructor]
        public TemplatedRunSettingsService(
            IRunSettingsTemplate runSettingsTemplate,
            ICustomRunSettingsTemplateProvider customRunSettingsTemplateProvider,
            IRunSettingsTemplateReplacementsFactory runSettingsTemplateReplacementsFactory,
            IProjectRunSettingsGenerator projectRunSettingsGenerator)
        {
            _runSettingsTemplate = runSettingsTemplate;
            _customRunSettingsTemplateProvider = customRunSettingsTemplateProvider;
            _runSettingsTemplateReplacementsFactory = runSettingsTemplateReplacementsFactory;
            _projectRunSettingsGenerator = projectRunSettingsGenerator;
        }

        public List<TemplatedCoverageProjectRunSettingsResult> CreateProjectsRunSettings(
            IEnumerable<ICoverageProject> coverageProjects,
            string solutionDirectory,
            string fccMsTestAdapterPath) => coverageProjects.Select(coverageProject =>
            {
                string projectDirectory = Path.GetDirectoryName(coverageProject.ProjectFilePath);
                (string replaceableTemplate, string customTemplatePath) = GetRunSettingsTemplate(
                    projectDirectory, solutionDirectory);
                ITemplateReplacementResult templateReplaceResult = ReplaceTemplate(
                    coverageProject, replaceableTemplate, fccMsTestAdapterPath);

                return new TemplatedCoverageProjectRunSettingsResult
                {
                    CoverageProject = coverageProject,
                    RunSettings = templateReplaceResult.Replaced,
                    CustomTemplatePath = customTemplatePath,
                    ReplacedTestAdapter = templateReplaceResult.ReplacedTestAdapter,
                };
            }).ToList();

        private (string ReplaceableTemplate, string CustomPath) GetRunSettingsTemplate(string projectDirectory, string solutionDirectory)
        {
            string customPath = null;
            string replaceableTemplate;
            CustomRunSettingsTemplateDetails customRunSettingsTemplateDetails = _customRunSettingsTemplateProvider.Provide(projectDirectory, solutionDirectory);
            if (customRunSettingsTemplateDetails != null)
            {
                customPath = customRunSettingsTemplateDetails.Path;
                replaceableTemplate = _runSettingsTemplate.ConfigureCustom(customRunSettingsTemplateDetails.Template);
            }
            else
            {
                replaceableTemplate = _runSettingsTemplate.Get();
            }

            return (replaceableTemplate, customPath);
        }

        private ITemplateReplacementResult ReplaceTemplate(ICoverageProject coverageProject, string replaceableTemplate, string fccMsTestAdapterPath)
        {
            IRunSettingsTemplateReplacements replacements = _runSettingsTemplateReplacementsFactory.Create(coverageProject, fccMsTestAdapterPath);

            return _runSettingsTemplate.ReplaceTemplate(replaceableTemplate, replacements, coverageProject.IsDotNetFramework);
        }

        public Task CleanUpAsync(List<ICoverageProject> coverageProjects)
            => _projectRunSettingsGenerator.RemoveGeneratedProjectSettingsAsync(coverageProjects);
    }
}
