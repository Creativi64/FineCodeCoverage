using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Markdig.Renderers;
using Markdig.Syntax;

namespace FineCodeCoverage.Readme
{
    public abstract class NotifyingWpfRenderer : WpfRenderer
    {
        private List<INotifiyingObjectRenderer> _notifyingObjectRenderers;
        private readonly List<ElementAndMarker> _elementAndMarkers = new List<ElementAndMarker>();

        public IReadOnlyList<ElementAndMarker> ElementAndMarkers => this._elementAndMarkers;

        private void NotifyingObjectRenderer_CreatedEvent(object sender, List<ElementAndMarker> elementAndMarkers)
            => this._elementAndMarkers.AddRange(elementAndMarkers);

        public override void LoadDocument(FlowDocument document)
        {
            base.LoadDocument(document);
            document.Style = null;
        }

        public override object Render(MarkdownObject markdownObject)
        {
            IEnumerable<INotifiyingObjectRenderer> notifyingObjectRenderers = this.ObjectRenderers.OfType<INotifiyingObjectRenderer>();
            foreach (INotifiyingObjectRenderer notifyingObjectRenderer in notifyingObjectRenderers)
            {
                notifyingObjectRenderer.CreatedEvent += this.NotifyingObjectRenderer_CreatedEvent;
            }

            this._notifyingObjectRenderers = notifyingObjectRenderers.ToList();
            object rendered = base.Render(markdownObject);
            this._notifyingObjectRenderers.ForEach(notifyingObjectRenderer
                => notifyingObjectRenderer.CreatedEvent -= this.NotifyingObjectRenderer_CreatedEvent);
            return rendered;
        }
    }
}