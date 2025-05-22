using Markdig.Renderers;
using Markdig.Syntax;
using System;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    public class ThematicBreakRenderer : NotifyingObjectRenderer<ThematicBreakBlock>
    {
        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, ThematicBreakBlock obj)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var line = new System.Windows.Shapes.Line { X2 = 1 };

            var paragraph = new Paragraph
            {
                Inlines = { new InlineUIContainer(line) },
            };

            renderer.WriteBlock(paragraph);
            return new ElementAndMarker(line, MarkdownTypeMarker.ThematicBreakBlock);
        }
    }
}
