using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.Roslyn
{
    [Export(typeof(IRoslynService))]
    internal class RoslynService : IRoslynService
    {
        private readonly ILanguageContainingCodeVisitorFactory _languageContainingCodeVisitorFactory;
        private readonly ITextSnapshotToSyntaxService _textSnapshotToSyntaxService;

        [ImportingConstructor]
        public RoslynService(
            ILanguageContainingCodeVisitorFactory languageContainingCodeVisitorFactory,
            ITextSnapshotToSyntaxService textSnapshotToSyntaxService)
        {
            _languageContainingCodeVisitorFactory = languageContainingCodeVisitorFactory;
            _textSnapshotToSyntaxService = textSnapshotToSyntaxService;
        }
        public async Task<List<TextSpan>> GetContainingCodeSpansAsync(ITextSnapshot textSnapshot)
        {
            RootNodeAndLanguage rootNodeAndLanguage = await _textSnapshotToSyntaxService.GetRootAndLanguageAsync(textSnapshot);
            if (rootNodeAndLanguage == null)
            {
                return Enumerable.Empty<TextSpan>().ToList();
            }

            bool isCSharp = rootNodeAndLanguage.Language == LanguageNames.CSharp;
            ILanguageContainingCodeVisitor languageContainingCodeVisitor = _languageContainingCodeVisitorFactory.Create(isCSharp);
            return languageContainingCodeVisitor.GetSpans(rootNodeAndLanguage.Root);
        }
    }
}
