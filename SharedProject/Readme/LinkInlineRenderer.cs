using Markdig.Renderers.Wpf;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows;
using Markdig.Wpf;

namespace FineCodeCoverage.Readme
{
    public class LinkInlineRenderer : NotifyingObjectRenderer<LinkInline>
    {
        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, LinkInline link)
        {
            ElementAndMarker elementAndMarker = null;
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (link == null) throw new ArgumentNullException(nameof(link));

            var url = link.GetDynamicUrl != null ? link.GetDynamicUrl() ?? link.Url : link.Url;

            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                url = "#";
            }

            if (link.IsImage)
            {
                var template = new ControlTemplate();
                var image = new FrameworkElementFactory(typeof(Image));
                image.SetValue(Image.SourceProperty, new BitmapImage(new Uri(url, UriKind.RelativeOrAbsolute)));
                template.VisualTree = image;

                var btn = new Button()
                {
                    Template = template,
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
