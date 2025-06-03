using System;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal class CoverletExitCodeFailureException : Exception
    {
        public CoverletExitCodeFailureException(string message) : base(message)
        {
        }

        public CoverletExitCodeFailureException() : base()
        {
        }

        public CoverletExitCodeFailureException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}