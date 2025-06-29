using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Markdig.Renderers;
using Markdig.Syntax;

namespace MarkdigExtended.NotifyingWpfRenderers.Base
{
    public abstract class NotifyingWpfRenderer : WpfRenderer
    {
        private readonly List<ElementAndMarker> _elementAndMarkers = new List<ElementAndMarker>();

        public IReadOnlyList<ElementAndMarker> ElementAndMarkers => _elementAndMarkers;

        private void NotifyingObjectRenderer_CreatedEvent(object? sender, List<ElementAndMarker> elementAndMarkers)
            => _elementAndMarkers.AddRange(elementAndMarkers);

        public override void LoadDocument(FlowDocument document)
        {
            base.LoadDocument(document);
            document.Style = null;
        }

        public override object Render(MarkdownObject markdownObject)
        {
            IEnumerable<INotifiyingObjectRenderer> notifyingObjectRenderers = ObjectRenderers.OfType<INotifiyingObjectRenderer>();
            foreach (INotifiyingObjectRenderer notifyingObjectRenderer in notifyingObjectRenderers)
            {
                notifyingObjectRenderer.CreatedEvent += NotifyingObjectRenderer_CreatedEvent;
            }

            var _notifyingObjectRenderers = notifyingObjectRenderers.ToList();
            object rendered = base.Render(markdownObject)!;
            foreach (INotifiyingObjectRenderer notifyingObjectRenderer in notifyingObjectRenderers)
            {
                notifyingObjectRenderer.CreatedEvent -= NotifyingObjectRenderer_CreatedEvent;
            }

            return rendered;
        }
    }
}
