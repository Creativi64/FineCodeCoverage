using System.Windows.Input;
using FineCodeCoverage.Core.Utilities;
using Markdig.Renderers.Wpf;
using Markdig.Renderers.Wpf.Inlines;
using MarkdigExtended.NotifyingWpfRenderers.Base;
using NotifyingRenderers = MarkdigExtended.NotifyingWpfRenderers.Renderers;

namespace FineCodeCoverage.Readme
{
    internal sealed class FCCMarkdigWpfRenderer : NotifyingWpfRenderer
    {
        private readonly string _readMeDirectory;
        private readonly ICommand _navigateCommand;

        public FCCMarkdigWpfRenderer(string readMeDirectory, ICommand navigateCommand)
        {
            _readMeDirectory = readMeDirectory;
            _navigateCommand = navigateCommand;
        }

        protected override void LoadRenderers()
        {
            // Notifying object renderers
            ObjectRenderers.Add(new NotifyingRenderers.CodeBlockRenderer());
            ObjectRenderers.Add(new NotifyingRenderers.HeadingRenderer());
            ObjectRenderers.Add(new NotifyingRenderers.ParagraphRenderer());
            ObjectRenderers.Add(new NotifyingRenderers.QuoteBlockRenderer());
            ObjectRenderers.Add(new NotifyingRenderers.ThematicBreakRenderer());
            ObjectRenderers.Add(new NotifyingRenderers.TableRenderer());
            ObjectRenderers.Add(new NotifyingRenderers.TaskListRenderer());
            ObjectRenderers.Add(new NotifyingRenderers.CodeInlineRenderer());
            ObjectRenderers.Add(new NotifyingRenderers.EmphasisInlineRenderer());
            ObjectRenderers.Add(
                new NotifyingRenderers.LinkInlineRenderer(_readMeDirectory, FCCGithub.MasterBlob, _navigateCommand));
            ObjectRenderers.Add(new NotifyingRenderers.AutolinkInlineRenderer(_navigateCommand));

            // markdig
            ObjectRenderers.Add(new ListRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer());
            ObjectRenderers.Add(new DelimiterInlineRenderer());
            ObjectRenderers.Add(new HtmlEntityInlineRenderer());
            ObjectRenderers.Add(new LineBreakInlineRenderer());
        }
    }
}
