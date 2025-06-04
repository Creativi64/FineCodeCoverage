using System.Windows.Input;
using FineCodeCoverage.Core.Utilities;
using Markdig.Renderers.Wpf;
using Markdig.Renderers.Wpf.Inlines;

namespace FineCodeCoverage.Readme
{
    internal class FCCMarkdigWpfRenderer : NotifyingWpfRenderer
    {
        private readonly string _readMeDirectory;
        private readonly ICommand _navigateCommand;

        public FCCMarkdigWpfRenderer(string readMeDirectory, ICommand navigateCommand)
        {
            this._readMeDirectory = readMeDirectory;
            this._navigateCommand = navigateCommand;
        }

        protected override void LoadRenderers()
        {
            // Notifying object renderers
            this.ObjectRenderers.Add(new CodeBlockRenderer());
            this.ObjectRenderers.Add(new HeadingRenderer());
            this.ObjectRenderers.Add(new ParagraphRenderer());
            this.ObjectRenderers.Add(new QuoteBlockRenderer());
            this.ObjectRenderers.Add(new ThematicBreakRenderer());
            this.ObjectRenderers.Add(new TableRenderer());
            this.ObjectRenderers.Add(new TaskListRenderer());
            this.ObjectRenderers.Add(new CodeInlineRenderer());
            this.ObjectRenderers.Add(new EmphasisInlineRenderer());
            this.ObjectRenderers.Add(
                new LinkInlineRenderer(this._readMeDirectory, FCCGithub.MasterBlob, this._navigateCommand)
            );
            this.ObjectRenderers.Add(new AutolinkInlineRenderer(this._navigateCommand));

            //markdig
            this.ObjectRenderers.Add(new ListRenderer());
            this.ObjectRenderers.Add(new LiteralInlineRenderer());
            this.ObjectRenderers.Add(new DelimiterInlineRenderer());
            this.ObjectRenderers.Add(new HtmlEntityInlineRenderer());
            this.ObjectRenderers.Add(new LineBreakInlineRenderer());
        }
    }
}
