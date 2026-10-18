using FineCodeCoverage.Collection.Engine;

namespace FineCodeCoverage.Collection.Messages
{
    public sealed class EmptyCoverageMessage
    {
        public EmptyCoverageMessage(CoverageCollectionResult collection) => Collection = collection;

        public CoverageCollectionResult Collection { get; }
    }
}
