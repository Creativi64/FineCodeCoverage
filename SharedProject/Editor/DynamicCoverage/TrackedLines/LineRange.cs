using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class LineRange
    {
        public LineRange(int startLineNumber, int endLineNumber)
        {
            StartLineNumber = startLineNumber;
            EndLineNumber = endLineNumber;
        }

        public int StartLineNumber { get; }

        public int EndLineNumber { get; }

        [ExcludeFromCodeCoverage]
        public override bool Equals(object obj)
            => obj is LineRange other &&
            other.StartLineNumber == StartLineNumber && other.EndLineNumber == EndLineNumber;

        [ExcludeFromCodeCoverage]
        public override int GetHashCode()
        {
            int hashCode = -414942;
            hashCode = (hashCode * -1521134295) + StartLineNumber.GetHashCode();
            hashCode = (hashCode * -1521134295) + EndLineNumber.GetHashCode();
            return hashCode;
        }
    }
}
