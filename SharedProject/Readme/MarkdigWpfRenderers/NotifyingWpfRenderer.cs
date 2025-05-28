using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Markdig.Renderers;
using Markdig.Syntax;

namespace FineCodeCoverage.Readme
{
    public abstract class NotifyingWpfRenderer : WpfRenderer
    {
        public List<ElementAndMarker> ElementAndMarkers = new List<ElementAndMarker>();
        private List<INotifiyingObjectRenderer> notifyingObjectRenderers;

        private void NotifyingObjectRenderer_CreatedEvent(object sender, List<ElementAndMarker> elementAndMarkers)
            => ElementAndMarkers.AddRange(elementAndMarkers);

        public override void LoadDocument(FlowDocument document)
        {
            base.LoadDocument(document);
            document.Style = null;
        }

        public override object Render(MarkdownObject markdownObject)
        {
            var notifyingObjectRenderers = ObjectRenderers.OfType<INotifiyingObjectRenderer>();
            foreach(var notifyingObjectRenderer in notifyingObjectRenderers)
            {
                notifyingObjectRenderer.CreatedEvent += NotifyingObjectRenderer_CreatedEvent;
            }
            this.notifyingObjectRenderers = notifyingObjectRenderers.ToList();
            var rendered = base.Render(markdownObject);
            this.notifyingObjectRenderers.ForEach(notifyingObjectRenderer
                => notifyingObjectRenderer.CreatedEvent -= NotifyingObjectRenderer_CreatedEvent);
            return rendered;
        }
    }
}
