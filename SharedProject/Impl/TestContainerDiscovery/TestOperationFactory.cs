using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using ReflectObject;

namespace FineCodeCoverage.Impl
{
    internal class TestContainerDiscoveryReflectionException : Exception
    {
        public TestContainerDiscoveryReflectionException(string message) : base(message)
        {
        }

        public TestContainerDiscoveryReflectionException() : base()
        {
        }

        public TestContainerDiscoveryReflectionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    [Export(typeof(ITestOperationFactory))]
    internal class TestOperationFactory : ITestOperationFactory
    {
        private readonly ICoverageProjectFactory coverageProjectFactory;
        private readonly IRunSettingsRetriever runSettingsRetriever;

        [ImportingConstructor]
        public TestOperationFactory(
            ICoverageProjectFactory coverageProjectFactory,
            IRunSettingsRetriever runSettingsRetriever
            )
        {
            this.coverageProjectFactory = coverageProjectFactory;
            this.runSettingsRetriever = runSettingsRetriever;
        }
        public ITestOperation Create(IOperation operation)
        {
            try
            {
                return new TestOperation(new TestRunRequest(operation), this.coverageProjectFactory, this.runSettingsRetriever);
            }
            catch (PropertyDoesNotExistException propertyDoesNotExistException)
            {
                throw new TestContainerDiscoveryReflectionException(propertyDoesNotExistException.Message);
            }
        }
    }
}

