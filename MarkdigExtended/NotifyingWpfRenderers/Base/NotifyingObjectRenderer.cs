using Markdig.Renderers;
using Markdig.Renderers.Wpf;
using Markdig.Syntax;

namespace MarkdigExtended.NotifyingWpfRenderers.Base
{
    public class NotifyingObjectRenderer<TObject> : WpfObjectRenderer<TObject>, INotifiyingObjectRenderer
        where TObject : MarkdownObject
    {
        public event EventHandler<List<ElementAndMarker>>? CreatedEvent;

        protected sealed override void Write(WpfRenderer renderer, TObject obj)
        {
            var element = WriteAndReturn(renderer, obj);
            var elementMarkers = element != null ?
                new List<ElementAndMarker> { element } : WriteAndReturns(renderer, obj);
            if (elementMarkers == null)
            {
                return;
            }

            CreatedEvent?.Invoke(this, elementMarkers);
        }

        protected virtual ElementAndMarker? WriteAndReturn(WpfRenderer renderer, TObject obj) => null;

        protected virtual List<ElementAndMarker>? WriteAndReturns(WpfRenderer renderer, TObject obj) => null;
    }
}
