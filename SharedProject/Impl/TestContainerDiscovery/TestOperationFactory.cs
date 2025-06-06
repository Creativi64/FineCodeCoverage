using System.ComponentModel.Composition;
using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using ReflectObject;

namespace FineCodeCoverage.Impl
{
    [Export(typeof(ITestOperationFactory))]
    internal sealed class TestOperationFactory : ITestOperationFactory
    {
        private readonly ICoverageProjectFactory _coverageProjectFactory;
        private readonly IRunSettingsRetriever _runSettingsRetriever;

        [ImportingConstructor]
        public TestOperationFactory(
            ICoverageProjectFactory coverageProjectFactory,
            IRunSettingsRetriever runSettingsRetriever)
        {
            _coverageProjectFactory = coverageProjectFactory;
            _runSettingsRetriever = runSettingsRetriever;
        }

        public ITestOperation Create(IOperation operation)
        {
            try
            {
                return new TestOperation(new TestRunRequest(operation), _coverageProjectFactory, _runSettingsRetriever);
            }
            catch (PropertyDoesNotExistException propertyDoesNotExistException)
            {
                throw new TestContainerDiscoveryReflectionException(propertyDoesNotExistException.Message);
            }
        }
    }
}
