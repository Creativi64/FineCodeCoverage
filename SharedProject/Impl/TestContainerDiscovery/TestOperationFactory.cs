using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using ReflectObject;
using ILogger = FineCodeCoverage.Output.ILogger;

namespace FineCodeCoverage.Impl
{
    [Export(typeof(ITestOperationFactory))]
    internal class TestOperationFactory : ITestOperationFactory
    {
        private readonly ICoverageProjectFactory coverageProjectFactory;
        private readonly IRunSettingsRetriever runSettingsRetriever;
        private readonly ILogger logger;

        [ImportingConstructor]
        public TestOperationFactory(
            ICoverageProjectFactory coverageProjectFactory,
            IRunSettingsRetriever runSettingsRetriever,
            ILogger logger
            )
        {
            this.coverageProjectFactory = coverageProjectFactory;
            this.runSettingsRetriever = runSettingsRetriever;
            this.logger = logger;
        }
        public async Task<ITestOperation> CreateAsync(IOperation operation)
        {
            try
            {
                return new TestOperation(new TestRunRequest(operation), this.coverageProjectFactory, this.runSettingsRetriever);
            }
            catch (PropertyDoesNotExistException propertyDoesNotExistException)
            {
                await this.logger.LogAsync("Error test container discoverer reflection");
                throw new Exception(propertyDoesNotExistException.Message);
            }
        }
    }
}

