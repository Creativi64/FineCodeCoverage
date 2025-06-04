using System;

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
}
