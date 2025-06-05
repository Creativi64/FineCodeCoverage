using System;
using System.Windows.Documents;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace FineCodeCoverage.Readme
{
    public class EmphasisInlineRenderer : NotifyingObjectRenderer<EmphasisInline>
    {
        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, EmphasisInline obj)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            MarkdownTypeMarker? markdownTypeMarker = null;

            Span span = null;
            if (obj.DelimiterChar == '*' || obj.DelimiterChar == '_')
            {
                span = obj.DelimiterCount == 2 ? (Span)new Bold() : new Italic();
            }
            else
            {
                switch (obj.DelimiterChar)
                {
                    case '~':
                        markdownTypeMarker = obj.DelimiterCount == 2 ?
                            MarkdownTypeMarker.EmphasisInlineStrikeThrough :
                            MarkdownTypeMarker.EmphasisInlineSubscript;
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

                if (markdownTypeMarker.HasValue)
                {
                    span = new Span();
                }
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

            return markdownTypeMarker.HasValue ? new ElementAndMarker(span, markdownTypeMarker.Value) : null;
        }
    }
}
