using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using System;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    public class EmphasisInlineRenderer : NotifyingObjectRenderer<EmphasisInline>
    {
        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, EmphasisInline obj)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            Span span = null;
            MarkdownTypeMarker? markdownTypeMarker = null;
            if (obj.DelimiterChar == '*' || obj.DelimiterChar == '_')
            {
                span = obj.DelimiterCount == 2 ? (Span)new Bold() : new Italic();
            }
            else
            {
                switch (obj.DelimiterChar)
                {
                    case '~':
                        markdownTypeMarker = obj.DelimiterCount == 2 ? MarkdownTypeMarker.EmphasisInlineStrikeThrough : MarkdownTypeMarker.EmphasisInlineSubscript;
                        break;
                    case '^':
                        markdownTypeMarker = MarkdownTypeMarker.EmphasisInlineSuperscript;
                        break;
                    case '+':
                        markdownTypeMarker = MarkdownTypeMarker.EmphasisInlineInserted;
                        break;
                    case '=':
                        markdownTypeMarker = MarkdownTypeMarker.EmphasisInlineMarked;
                        break;
                }
                span = new Span();
            }


            if (span != null)
            {
                renderer.Push(span);
                renderer.WriteChildren(obj);
                renderer.Pop();
            }
            else
            {
                renderer.WriteChildren(obj);
            }
            if(markdownTypeMarker.HasValue)
            {
                return new ElementAndMarker(span, markdownTypeMarker.Value);
            }
            return null;
        }
    }
}
