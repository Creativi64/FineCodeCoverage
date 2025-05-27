using System;
using System.Windows.Documents;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace FineCodeCoverage.Readme
{
    public class CodeInlineRenderer : NotifyingObjectRenderer<CodeInline>
    {
        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, CodeInline obj)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var run = new Run(obj.Content);
            renderer.WriteInline(run);
            return new ElementAndMarker(run, MarkdownTypeMarker.CodeInline);
        }
    }
}
