using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class CodeSpanRange
    {
        public CodeSpanRange(int startLine, int endLine)
        {
            StartLine = startLine;
            EndLine = endLine;
        }

        public static CodeSpanRange SingleLine(int lineNumber) => new CodeSpanRange(lineNumber, lineNumber);

        public int StartLine { get; set; }

        public int EndLine { get; set; }

        [ExcludeFromCodeCoverage]
        public override bool Equals(object obj)
            => obj is CodeSpanRange codeSpanRange && codeSpanRange.StartLine == StartLine && codeSpanRange.EndLine == EndLine;

        [ExcludeFromCodeCoverage]
        public override int GetHashCode()
        {
            int hashCode = -1763436595;
            hashCode = (hashCode * -1521134295) + StartLine.GetHashCode();
            hashCode = (hashCode * -1521134295) + EndLine.GetHashCode();
            return hashCode;
        }
    }
}
