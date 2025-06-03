using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class LineRange
    {
        public LineRange(int startLineNumber, int endLineNumber)
        {
            this.StartLineNumber = startLineNumber;
            this.EndLineNumber = endLineNumber;
        }

        public int StartLineNumber { get; }
        public int EndLineNumber { get; }

        [ExcludeFromCodeCoverage]
        public override bool Equals(object obj)
            => obj is LineRange other &&
            other.StartLineNumber == this.StartLineNumber && other.EndLineNumber == this.EndLineNumber;

        [ExcludeFromCodeCoverage]
        public override int GetHashCode()
        {
            int hashCode = -414942;
            hashCode = (hashCode * -1521134295) + this.StartLineNumber.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.EndLineNumber.GetHashCode();
            return hashCode;
        }
    }
}