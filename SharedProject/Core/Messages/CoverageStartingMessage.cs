namespace FineCodeCoverage.Engine.Messages
{
    internal class CoverageStartingMessage
    {
        public CoverageStartingMessage(bool pending = false) => this.Pending = pending;

        public bool Pending { get; }
    }
}