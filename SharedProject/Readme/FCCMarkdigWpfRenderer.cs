using System.Windows.Input;
using FineCodeCoverage.Core.Utilities;
using Markdig.Renderers.Wpf;
using Markdig.Renderers.Wpf.Inlines;

namespace FineCodeCoverage.Readme
{
    internal class FCCMarkdigWpfRenderer : NotifyingWpfRenderer
    {
        private readonly string readMeDirectory;
        private readonly ICommand navigateCommand;

        public FCCMarkdigWpfRenderer(string readMeDirectory, ICommand navigateCommand)
        {
            this.readMeDirectory = readMeDirectory;
            this.navigateCommand = navigateCommand;
        }

        protected override void LoadRenderers()
        {
            // Notifying object renderers
            ObjectRenderers.Add(new CodeBlockRenderer());
            ObjectRenderers.Add(new HeadingRenderer());
            ObjectRenderers.Add(new ParagraphRenderer());
            ObjectRenderers.Add(new QuoteBlockRenderer());
            ObjectRenderers.Add(new ThematicBreakRenderer());
            ObjectRenderers.Add(new TableRenderer());
            ObjectRenderers.Add(new TaskListRenderer());
            ObjectRenderers.Add(new CodeInlineRenderer());
            ObjectRenderers.Add(new EmphasisInlineRenderer());
            ObjectRenderers.Add(new LinkInlineRenderer(this.readMeDirectory, FCCGithub.MasterBlob, this.navigateCommand));
            ObjectRenderers.Add(new AutolinkInlineRenderer(this.navigateCommand));

            //markdig
            ObjectRenderers.Add(new ListRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer());
            ObjectRenderers.Add(new DelimiterInlineRenderer());
            ObjectRenderers.Add(new HtmlEntityInlineRenderer());
            ObjectRenderers.Add(new LineBreakInlineRenderer());
        }
    }
}
