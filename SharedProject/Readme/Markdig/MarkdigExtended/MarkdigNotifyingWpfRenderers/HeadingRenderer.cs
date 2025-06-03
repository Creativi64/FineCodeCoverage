using System;
using System.Windows.Documents;
using Markdig.Renderers;
using Markdig.Syntax;

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

        private static MarkdownTypeMarker GetMarker(int level)
        {
            switch (level)
            {
                case 1:
                    return MarkdownTypeMarker.HeadingBlock1;
                case 2:
                    return MarkdownTypeMarker.HeadingBlock2;
                case 3:
                    return MarkdownTypeMarker.HeadingBlock3;
                case 4:
                    return MarkdownTypeMarker.HeadingBlock4;
                case 5:
                    return MarkdownTypeMarker.HeadingBlock5;
                case 6:
                    return MarkdownTypeMarker.HeadingBlock6;
            }

            return MarkdownTypeMarker.HeadingBlock1;
        }
    }
}