namespace FineCodeCoverage.Engine.Messages
{
    internal sealed class CoverageStartingMessage
    {
        public CoverageStartingMessage(bool pending = false) => Pending = pending;

        public bool Pending { get; }
    }
}
