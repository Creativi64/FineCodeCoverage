using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Impl
{
    internal sealed class TestOperation : ITestOperation
    {
        private readonly TestRunRequest _testRunRequest;
        private readonly ICoverageProjectFactory _coverageProjectFactory;
        private readonly IRunSettingsRetriever _runSettingsRetriever;

        public TestOperation(TestRunRequest testRunRequest, ICoverageProjectFactory coverageProjectFactory, IRunSettingsRetriever runSettingsRetriever)
        {
            _testRunRequest = testRunRequest;
            _coverageProjectFactory = coverageProjectFactory;
            _runSettingsRetriever = runSettingsRetriever;
        }

        public long FailedTests => _testRunRequest.Response.FailedTests;

        public long TotalTests => _testRunRequest.TotalTests;

        public string SolutionDirectory => _testRunRequest.Configuration.SolutionDirectory;

        public Task<List<ICoverageProject>> GetCoverageProjectsAsync()
            => GetCoverageProjectsAsync(_testRunRequest.Configuration);

        private async Task<List<ICoverageProject>> GetCoverageProjectsAsync(TestConfiguration testConfiguration)
        {
            object userRunSettings = testConfiguration.UserRunSettings;
            IEnumerable<Container> testContainers = testConfiguration.Containers;
            var coverageProjects = new List<ICoverageProject>();
            foreach (Container container in testContainers)
            {
                ICoverageProject project = _coverageProjectFactory.Create();
                coverageProjects.Add(project);
                project.ProjectName = container.ProjectName;
                project.TestDllFile = container.Source;
                project.Is64Bit = container.TargetPlatform.ToString().Equals("x64", System.StringComparison.OrdinalIgnoreCase);
                project.TargetFramework = container.TargetFramework.ToString();
                ContainerData containerData = container.ProjectData;
                project.ProjectFilePath = container.ProjectData.ProjectFilePath;
                project.Id = containerData.Id;
                project.RunSettingsFile = await _runSettingsRetriever.GetRunSettingsFileAsync(userRunSettings, containerData);
            }

            return coverageProjects;
        }
    }
}
