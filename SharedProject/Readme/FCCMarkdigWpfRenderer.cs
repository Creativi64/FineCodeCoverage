using Markdig.Renderers.Wpf;
using Markdig.Renderers.Wpf.Inlines;
using System.Collections.Generic;

namespace FineCodeCoverage.Readme
{
    internal class FCCMarkdigWpfRenderer : NotifyingWpfRenderer
    {
        protected override List<INotifiyingObjectRenderer> LoadNotifyingObjectRenderers()
            => new List<INotifiyingObjectRenderer>
            {
                new CodeBlockRenderer(),
                new HeadingRenderer(),
                new ParagraphRenderer(),
                new QuoteBlockRenderer(),
                new ThematicBreakRenderer(),
                new TableRenderer(),
                new TaskListRenderer(),

                new CodeInlineRenderer(),
                new EmphasisInlineRenderer(),
                new AutolinkInlineRenderer()
            };

        protected override void LoadNonNotifyingObjectRenderers()
        {
            ObjectRenderers.Add(new ListRenderer());//markdig

            ObjectRenderers.Add(new LinkInlineRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer()); // markdig
            ObjectRenderers.Add(new DelimiterInlineRenderer()); // markdig
            ObjectRenderers.Add(new HtmlEntityInlineRenderer());// markdig
            ObjectRenderers.Add(new LineBreakInlineRenderer()); // markdig
        }
    }
}
