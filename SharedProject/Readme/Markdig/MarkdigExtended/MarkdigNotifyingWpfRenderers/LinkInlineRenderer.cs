using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace FineCodeCoverage.Readme
{
    public class LinkInlineRenderer : NotifyingObjectRenderer<LinkInline>
    {
        private readonly string relativeRoot;
        private readonly string githubRoot;
        private readonly ICommand navigateCommand;

        public LinkInlineRenderer(string relativeRoot, string githubRoot, ICommand navigateCommand)
        {
            this.relativeRoot = relativeRoot;
            this.githubRoot = githubRoot;
            this.navigateCommand = navigateCommand;
        }

        private string EnsureAbsolute(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                string localPath = Path.Combine(this.relativeRoot, url);
                return File.Exists(localPath) ?
                    localPath : new Uri(Path.Combine(this.githubRoot, url), UriKind.RelativeOrAbsolute).ToString();
            }

            return url;
        }

        private string GetUrl(LinkInline link)
        {
            string url = link.GetDynamicUrl != null ? link.GetDynamicUrl() ?? link.Url : link.Url;

            return !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute) ? "#" : this.EnsureAbsolute(url);
        }

        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, LinkInline link)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (link == null) throw new ArgumentNullException(nameof(link));

            string url = this.GetUrl(link);
            ElementAndMarker elementAndMarker;
            if (link.IsImage)
            {
                string altText = (link.FirstChild as LiteralInline)?.Content.ToString() ?? string.Empty;
                var image = new Image
                {
                    Source = new BitmapImage(new Uri(url, UriKind.RelativeOrAbsolute)),
                    Tag = altText
                };
                ICommand command = null;
                if (link.Parent is LinkInline urlLinkInline)
                {
                    command = this.navigateCommand;
                    url = this.GetUrl(urlLinkInline);
                }

                var btn = new Button()
                {
                    Content = image,
                    Command = command,
                    CommandParameter = url,
                };
                var inlineUIContainer = new InlineUIContainer(btn);
                elementAndMarker = new ElementAndMarker(image, MarkdownTypeMarker.LinkInlineImage);
                renderer.WriteInline(inlineUIContainer);
            }
            else
            {
                var hyperlink = new Hyperlink
                {
                    Command = this.navigateCommand,
                    CommandParameter = url,
                    NavigateUri = new Uri(url, UriKind.RelativeOrAbsolute),
                };
                elementAndMarker = new ElementAndMarker(hyperlink, MarkdownTypeMarker.LinkInlineHyperlink);

                renderer.Push(hyperlink);
                renderer.WriteChildren(link);
                renderer.Pop();
            }

            return elementAndMarker;
        }
    }
}