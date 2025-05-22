using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Wpf;
using Markdig.Renderers.Wpf.Inlines;
using Markdig.Syntax;
using System.Collections.Generic;
using System.Windows.Documents;
using Md = Markdig.Wpf.Markdown;

namespace FineCodeCoverage.Readme
{
    internal static class FCCMarkdownToFlowDocumentConverter
    {
        public static (FlowDocument flowDocument, List<ElementAndMarker> elementAndMarkers) MarkdownToFlowDocument(string markdown)
        {
            var fccRenderer = new FCCRenderer();
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
            var flowDocument =  Md.ToFlowDocument(markdown, pipeline, fccRenderer);
            return (flowDocument, fccRenderer.ElementAndMarkers);
        }
    }

    // might be able to construct with styles
    // although cannot get the key from the style after get from resources
    // can still get them from a key in advance - typed to Markdown types
    // base type that will accept a callback when has created with tags or virtual method with what to do with it
    internal class FCCRenderer : WpfRenderer
    {
        public List<ElementAndMarker> ElementAndMarkers = new List<ElementAndMarker>();
        private List<INotifiyingObjectRenderer> notifyingObjectRenderers;

        protected override void LoadRenderers()
        {
            // add the ones that FCC is interested in
            notifyingObjectRenderers = new List<INotifiyingObjectRenderer>
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
                new AutolinkInlineRenderer()
            };
            notifyingObjectRenderers.ForEach(notifyingObjectRenderer =>
            {
                ObjectRenderers.Add(notifyingObjectRenderer);
                notifyingObjectRenderer.CreatedEvent += NotifyingObjectRenderer_CreatedEvent;
            });
            ObjectRenderers.Add(new ListRenderer());//markdig

            ObjectRenderers.Add(new LinkInlineRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer()); // markdig
            ObjectRenderers.Add(new DelimiterInlineRenderer()); // markdig
            ObjectRenderers.Add(new HtmlEntityInlineRenderer());// markdig
            ObjectRenderers.Add(new LineBreakInlineRenderer()); // markdig
        }

        private void NotifyingObjectRenderer_CreatedEvent(object sender, List<ElementAndMarker> elementAndMarkers)
            => ElementAndMarkers.AddRange(elementAndMarkers);

        public override void LoadDocument(FlowDocument document)
        {
            base.LoadDocument(document);
            document.Style = null;
        }

        public override object Render(MarkdownObject markdownObject)
        {
            var flowDocument =  base.Render(markdownObject);
            notifyingObjectRenderers.ForEach(notifyingObjectRenderer
                => notifyingObjectRenderer.CreatedEvent -= NotifyingObjectRenderer_CreatedEvent);
            return flowDocument;
        }
    }
}
