using System;

namespace FineCodeCoverage.Engine.OpenCover
{
    internal class OpenCoverExitCodeException : Exception
    {
        public OpenCoverExitCodeException(string message)
            : base(message)
        {
        }

        public OpenCoverExitCodeException()
            : base()
        {
        }

        public OpenCoverExitCodeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
