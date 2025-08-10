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

        private sealed class ProjectRunSettingsFromTemplateResult : IProjectRunSettingsFromTemplateResult
        {
            private sealed class ExceptionReasonImpl : IExceptionReason
            {
                public ExceptionReasonImpl(Exception exc, string reason)
                {
                    Exception = exc;
                    Reason = reason;
                }

                public Exception Exception { get; }

                public string Reason { get; }
            }

            public IExceptionReason ExceptionReason { get; set; }

            public List<string> CustomTemplatePaths { get; set; }

            public List<ICoverageProject> CoverageProjectsWithFCCMsTestAdapter { get; set; }

            public static ProjectRunSettingsFromTemplateResult FromException(Exception exception, string reason)
                => new ProjectRunSettingsFromTemplateResult
                {
                    ExceptionReason = new ExceptionReasonImpl(exception, reason),
                };
        }

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

        public async Task<IProjectRunSettingsFromTemplateResult> GenerateAsync(IEnumerable<ICoverageProject> coverageProjectsWithoutRunSettings, string solutionDirectory, string fccMsTestAdapterPath)
        {
            IEnumerable<TemplatedCoverageProjectRunSettingsResult> projectsRunSettings;
            try
            {
                projectsRunSettings = CreateProjectsRunSettings(
                    coverageProjectsWithoutRunSettings, solutionDirectory, fccMsTestAdapterPath);
            }
            catch (Exception exc)
            {
                return ProjectRunSettingsFromTemplateResult.FromException(exc, "Exception generating runsettings from template");
            }

            try
            {
                await _projectRunSettingsGenerator.WriteProjectsRunSettingsAsync(projectsRunSettings);
            }
            catch (Exception exc)
            {
                try
                {
                    await _projectRunSettingsGenerator.RemoveGeneratedProjectSettingsAsync(coverageProjectsWithoutRunSettings);
                }
                catch
                {
                }

                return ProjectRunSettingsFromTemplateResult.FromException(exc, "Exception writing templated runsettings");
            }

            return CreateSuccessResult(projectsRunSettings);
        }

        private static ProjectRunSettingsFromTemplateResult CreateSuccessResult(
            IEnumerable<TemplatedCoverageProjectRunSettingsResult> templatedCoverageProjectsRunSettingsResult)
        {
            var customTemplatePaths = new List<string>();
            var coverageProjectsWithFCCMsTestAdapter = new List<ICoverageProject>();
            foreach (TemplatedCoverageProjectRunSettingsResult templatedCoverageProjectRunSettingsResult in templatedCoverageProjectsRunSettingsResult)
            {
                if (templatedCoverageProjectRunSettingsResult.ReplacedTestAdapter)
                {
                    coverageProjectsWithFCCMsTestAdapter.Add(templatedCoverageProjectRunSettingsResult.CoverageProject);
                }

                if (templatedCoverageProjectRunSettingsResult.CustomTemplatePath != null)
                {
                    customTemplatePaths.Add(templatedCoverageProjectRunSettingsResult.CustomTemplatePath);
                }
            }

            return new ProjectRunSettingsFromTemplateResult
            {
                CustomTemplatePaths = customTemplatePaths,
                CoverageProjectsWithFCCMsTestAdapter = coverageProjectsWithFCCMsTestAdapter,
            };
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
