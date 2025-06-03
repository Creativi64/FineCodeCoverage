using System;
using System.Windows.Documents;
using System.Windows.Input;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace FineCodeCoverage.Readme
{
    public class AutolinkInlineRenderer : NotifyingObjectRenderer<AutolinkInline>
    {
        private readonly ICommand navigateCommand;

        public AutolinkInlineRenderer(ICommand navigateCommand) => this.navigateCommand = navigateCommand;

        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, AutolinkInline link)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (link == null) throw new ArgumentNullException(nameof(link));

            string url = link.Url;
            if (link.IsEmail)
            {
                url = "mailto:" + url;
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                url = "#";
            }

            var hyperlink = new Hyperlink
            {
                Command = this.navigateCommand,
                CommandParameter = url,
                NavigateUri = new Uri(url, UriKind.RelativeOrAbsolute),
                ToolTip = link.Url,
            };

            renderer.Push(hyperlink);
            renderer.WriteText(link.Url);
            renderer.Pop();
            return new ElementAndMarker(hyperlink, MarkdownTypeMarker.AutolinkInline);
        }
    }
}