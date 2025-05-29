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
            => this.ElementAndMarkers.AddRange(elementAndMarkers);

        public override void LoadDocument(FlowDocument document)
        {
            base.LoadDocument(document);
            document.Style = null;
        }

        public override object Render(MarkdownObject markdownObject)
        {
            IEnumerable<INotifiyingObjectRenderer> notifyingObjectRenderers = this.ObjectRenderers.OfType<INotifiyingObjectRenderer>();
            foreach(INotifiyingObjectRenderer notifyingObjectRenderer in notifyingObjectRenderers)
            {
                notifyingObjectRenderer.CreatedEvent += this.NotifyingObjectRenderer_CreatedEvent;
            }

            this.notifyingObjectRenderers = notifyingObjectRenderers.ToList();
            object rendered = base.Render(markdownObject);
            this.notifyingObjectRenderers.ForEach(notifyingObjectRenderer
                => notifyingObjectRenderer.CreatedEvent -= this.NotifyingObjectRenderer_CreatedEvent);
            return rendered;
        }
    }
}
