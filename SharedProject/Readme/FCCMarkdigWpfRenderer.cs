using System.Collections.Generic;
using System.Windows.Input;
using FineCodeCoverage.Core.Utilities;

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
                new LinkInlineRenderer(this.readMeDirectory, FCCGithub.MasterBlob,this.navigateCommand),
                new AutolinkInlineRenderer(this.navigateCommand)
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
