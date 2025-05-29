using System;
using System.Windows.Documents;
using Markdig.Renderers;
using Markdig.Syntax;

namespace FineCodeCoverage.Readme
{
    public class ParagraphRenderer : NotifyingObjectRenderer<ParagraphBlock>
    {
        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, ParagraphBlock obj)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var paragraph = new Paragraph();

            renderer.Push(paragraph);
            renderer.WriteLeafInline(obj);
            renderer.Pop();
            return new ElementAndMarker(paragraph, MarkdownTypeMarker.Paragraph);
        }
    }
}
