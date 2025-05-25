using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Markdig.Wpf;
using System.IO;

namespace FineCodeCoverage.Readme
{
    public class LinkInlineRenderer : NotifyingObjectRenderer<LinkInline>
    {
        private readonly string relativeRoot;

        public LinkInlineRenderer(string relativeRoot)
        {
            this.relativeRoot = relativeRoot;
        }

        private string EnsureAbsolute(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                return Path.Combine(this.relativeRoot, url);
            }
            return url;
        }

        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, LinkInline link)
        {
            ElementAndMarker elementAndMarker = null;
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (link == null) throw new ArgumentNullException(nameof(link));

            var url = link.GetDynamicUrl != null ? link.GetDynamicUrl() ?? link.Url : link.Url;

            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                url = "#";
            }else
            {
                url = EnsureAbsolute(url);
            }

            if (link.IsImage)
            {
                var image = new Image
                {
                    Source = new BitmapImage(new Uri(url, UriKind.RelativeOrAbsolute)),
                };
                var btn = new Button()
                {
                    Content = image,
                    Command = Commands.Image,
                    CommandParameter = url
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
