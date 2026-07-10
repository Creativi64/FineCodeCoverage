using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace FineCodeCoverage.Editor.DynamicCoverage.Utilities
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(ISyntaxNodeLocationMapper))]
    internal sealed class SyntaxNodeLocationMapper : ISyntaxNodeLocationMapper
    {
        public FileLinePositionSpan Map(SyntaxNode node) => node.GetLocation().GetMappedLineSpan();
    }
}
