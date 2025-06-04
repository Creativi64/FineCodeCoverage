using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Impl
{
    internal class TestOperation : ITestOperation
    {
        private readonly TestRunRequest _testRunRequest;
        private readonly ICoverageProjectFactory _coverageProjectFactory;
        private readonly IRunSettingsRetriever _runSettingsRetriever;

        public TestOperation(TestRunRequest testRunRequest, ICoverageProjectFactory coverageProjectFactory, IRunSettingsRetriever runSettingsRetriever)
        {
            this._testRunRequest = testRunRequest;
            this._coverageProjectFactory = coverageProjectFactory;
            this._runSettingsRetriever = runSettingsRetriever;
        }

        public long FailedTests => this._testRunRequest.Response.FailedTests;

        public long TotalTests => this._testRunRequest.TotalTests;

        public string SolutionDirectory => this._testRunRequest.Configuration.SolutionDirectory;

        public Task<List<ICoverageProject>> GetCoverageProjectsAsync()
            => this.GetCoverageProjectsAsync(this._testRunRequest.Configuration);

        private async Task<List<ICoverageProject>> GetCoverageProjectsAsync(TestConfiguration testConfiguration)
        {
            object userRunSettings = testConfiguration.UserRunSettings;
            IEnumerable<Container> testContainers = testConfiguration.Containers;
            var coverageProjects = new List<ICoverageProject>();
            foreach (Container container in testContainers)
            {
                ICoverageProject project = this._coverageProjectFactory.Create();
                coverageProjects.Add(project);
                project.ProjectName = container.ProjectName;
                project.TestDllFile = container.Source;
                project.Is64Bit = container.TargetPlatform.ToString().Equals("x64", System.StringComparison.OrdinalIgnoreCase);
                project.TargetFramework = container.TargetFramework.ToString();
                ContainerData containerData = container.ProjectData;
                project.ProjectFilePath = container.ProjectData.ProjectFilePath;
                project.Id = containerData.Id;
                project.RunSettingsFile = await this._runSettingsRetriever.GetRunSettingsFileAsync(userRunSettings, containerData);
            }

            return coverageProjects;
        }
    }
}
