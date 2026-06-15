using System.ComponentModel.Composition;

namespace FineCodeCoverage.Editor.Roslyn
{
    [Export(typeof(ILanguageContainingCodeVisitorFactory))]
    internal sealed class LanguageContainingCodeVisitorFactory : ILanguageContainingCodeVisitorFactory
    {
        public ILanguageContainingCodeVisitor Create(bool isCSharp)
            => isCSharp ?
            new CSharpContainingCodeVisitor() :
            new VBContainingCodeVisitor();
    }
}
