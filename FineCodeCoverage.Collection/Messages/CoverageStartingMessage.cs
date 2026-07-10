namespace FineCodeCoverage.Collection.Messages
{
    public sealed class CoverageStartingMessage
    {
        public CoverageStartingMessage(bool pending = false) => Pending = pending;

        public bool Pending { get; }
    }
}
