using System;

namespace FineCodeCoverage.Collection.TestExplorer.InternalTypes
{
    internal sealed class TestContainerDiscoveryReflectionException : Exception
    {
        public TestContainerDiscoveryReflectionException(string message)
            : base(message)
        {
        }

        public TestContainerDiscoveryReflectionException()
            : base()
        {
        }

        public TestContainerDiscoveryReflectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
