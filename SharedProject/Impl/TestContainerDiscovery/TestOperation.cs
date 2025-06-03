using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Impl
{
    internal class TestOperation : ITestOperation
    {
        private readonly TestRunRequest testRunRequest;
        private readonly ICoverageProjectFactory coverageProjectFactory;
        private readonly IRunSettingsRetriever runSettingsRetriever;

        public TestOperation(TestRunRequest testRunRequest, ICoverageProjectFactory coverageProjectFactory, IRunSettingsRetriever runSettingsRetriever)
        {
            this.testRunRequest = testRunRequest;
            this.coverageProjectFactory = coverageProjectFactory;
            this.runSettingsRetriever = runSettingsRetriever;
        }
        public long FailedTests => this.testRunRequest.Response.FailedTests;

        public long TotalTests => this.testRunRequest.TotalTests;

        public string SolutionDirectory => this.testRunRequest.Configuration.SolutionDirectory;

        public Task<List<ICoverageProject>> GetCoverageProjectsAsync()
            => this.GetCoverageProjectsAsync(this.testRunRequest.Configuration);

        private async Task<List<ICoverageProject>> GetCoverageProjectsAsync(TestConfiguration testConfiguration)
        {
            object userRunSettings = testConfiguration.UserRunSettings;
            IEnumerable<Container> testContainers = testConfiguration.Containers;
            var coverageProjects = new List<ICoverageProject>();
            foreach (Container container in testContainers)
            {
                ICoverageProject project = this.coverageProjectFactory.Create();
                coverageProjects.Add(project);
                project.ProjectName = container.ProjectName;
                project.TestDllFile = container.Source;
                project.Is64Bit = container.TargetPlatform.ToString().Equals("x64", System.StringComparison.OrdinalIgnoreCase);
                project.TargetFramework = container.TargetFramework.ToString();
                ContainerData containerData = container.ProjectData;
                project.ProjectFilePath = container.ProjectData.ProjectFilePath;
                project.Id = containerData.Id;
                project.RunSettingsFile = await this.runSettingsRetriever.GetRunSettingsFileAsync(userRunSettings, containerData);
            }

            return coverageProjects;
        }
    }
}