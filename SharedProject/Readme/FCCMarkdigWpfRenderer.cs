using FineCodeCoverage.Core.Utilities;
using Markdig.Renderers.Wpf;
using Markdig.Renderers.Wpf.Inlines;
using System.Collections.Generic;

namespace FineCodeCoverage.Readme
{
    internal class FCCMarkdigWpfRenderer : NotifyingWpfRenderer
    {
        private readonly string readMeDirectory;

        public FCCMarkdigWpfRenderer(string readMeDirectory)
        {
            this.readMeDirectory = readMeDirectory;
        }
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
                new LinkInlineRenderer(readMeDirectory, FCCGithub.MasterBlob),
                new AutolinkInlineRenderer()
            };

        protected override void LoadNonNotifyingObjectRenderers()
        {
            ObjectRenderers.Add(new ListRenderer());//markdig

            ObjectRenderers.Add(new LiteralInlineRenderer()); // markdig
            ObjectRenderers.Add(new DelimiterInlineRenderer()); // markdig
            ObjectRenderers.Add(new HtmlEntityInlineRenderer());// markdig
            ObjectRenderers.Add(new LineBreakInlineRenderer()); // markdig
        }
    }
}
