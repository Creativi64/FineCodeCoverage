using System;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal class CoverletDataCollectorDidNotGenerateCoberturaException : Exception
    {
        public CoverletDataCollectorDidNotGenerateCoberturaException(string expectedCobertura)
            : base(expectedCobertura)
        {
        }

        public CoverletDataCollectorDidNotGenerateCoberturaException()
            : base()
        {
        }

        public CoverletDataCollectorDidNotGenerateCoberturaException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
