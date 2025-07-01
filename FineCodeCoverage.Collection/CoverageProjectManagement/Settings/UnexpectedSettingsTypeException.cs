using System;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    internal sealed class UnexpectedSettingsTypeException : Exception
    {
        public UnexpectedSettingsTypeException(string message)
            : base(message)
        {
        }

        public UnexpectedSettingsTypeException()
            : base()
        {
        }

        public UnexpectedSettingsTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
