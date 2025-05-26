using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Markdig.Wpf;
using System.IO;
using System.Windows.Input;

namespace FineCodeCoverage.Readme
{
    public class LinkInlineRenderer : NotifyingObjectRenderer<LinkInline>
    {
        private readonly string relativeRoot;
        private readonly string githubRoot;

        public LinkInlineRenderer(string relativeRoot,string githubRoot)
        {
            this.relativeRoot = relativeRoot;
            this.githubRoot = githubRoot;
        }

        private string EnsureAbsolute(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                var localPath = Path.Combine(this.relativeRoot, url);
                if (File.Exists(localPath))
                {
                    return localPath;
                }
                return new Uri(Path.Combine(this.githubRoot, url), UriKind.RelativeOrAbsolute).ToString();
            }
            return url;
        }

        private string GetUrl(LinkInline link)
        {
            var url = link.GetDynamicUrl != null ? link.GetDynamicUrl() ?? link.Url : link.Url;

            return !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute) ? "#" : EnsureAbsolute(url);
        }

        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, LinkInline link)
        {
            ElementAndMarker elementAndMarker = null;
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (link == null) throw new ArgumentNullException(nameof(link));

            var url = GetUrl(link);


            if (link.IsImage)
            {
                string altText = (link.FirstChild as LiteralInline)?.Content.ToString() ?? string.Empty;
                var image = new Image
                {
                    Source = new BitmapImage(new Uri(url, UriKind.RelativeOrAbsolute)),
                    Tag = altText
                };
                ICommand command = Commands.Image;
                if (link.Parent is LinkInline urlLinkInline)
                {
                    command = Commands.Hyperlink;
                    url = GetUrl(urlLinkInline);
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
                    Command = Commands.Hyperlink,
                    CommandParameter = url,
                    NavigateUri = new Uri(url, UriKind.RelativeOrAbsolute),
                    ToolTip = !string.IsNullOrEmpty(link.Title) ? link.Title : null,
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
