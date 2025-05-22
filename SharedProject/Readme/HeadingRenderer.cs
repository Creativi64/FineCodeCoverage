using Markdig.Renderers;
using Markdig.Syntax;
using System;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    public class HeadingRenderer : NotifyingObjectRenderer<HeadingBlock>
    {
        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, HeadingBlock obj)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var paragraph = new Paragraph();

            renderer.Push(paragraph);
            renderer.WriteLeafInline(obj);
            renderer.Pop();
            return new ElementAndMarker(paragraph, GetMarker(obj.Level));
        }

        private MarkdownTypeMarker GetMarker(int level)
        {
            MarkdownTypeMarker markdownTypeMarker = MarkdownTypeMarker.HeadingBlock1;
            switch (level)
            {
                case 1:
                    markdownTypeMarker = MarkdownTypeMarker.HeadingBlock1;
                    break;
                case 2:
                    markdownTypeMarker = MarkdownTypeMarker.HeadingBlock2;
                    break;
                case 3:
                    markdownTypeMarker = MarkdownTypeMarker.HeadingBlock3;
                    break;
                case 4:
                    markdownTypeMarker = MarkdownTypeMarker.HeadingBlock4;
                    break;
                case 5:
                    markdownTypeMarker = MarkdownTypeMarker.HeadingBlock5;
                    break;
                case 6:
                    markdownTypeMarker = MarkdownTypeMarker.HeadingBlock6;
                    break;
            }
            return markdownTypeMarker;
        }
    }

}
