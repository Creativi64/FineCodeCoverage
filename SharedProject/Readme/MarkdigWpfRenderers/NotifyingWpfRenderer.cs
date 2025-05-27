using System.Collections.Generic;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    public abstract class NotifyingWpfRenderer : WpfRenderer
    {
        public List<ElementAndMarker> ElementAndMarkers = new List<ElementAndMarker>();
        private List<INotifiyingObjectRenderer> notifyingObjectRenderers;

        protected override void LoadRenderers()
        {
            notifyingObjectRenderers = LoadNotifyingObjectRenderers();
            notifyingObjectRenderers.ForEach(notifyingObjectRenderer =>
            {
                ObjectRenderers.Add(notifyingObjectRenderer);
                notifyingObjectRenderer.CreatedEvent += NotifyingObjectRenderer_CreatedEvent;
            });
            LoadNonNotifyingObjectRenderers();
        }

        private void NotifyingObjectRenderer_CreatedEvent(object sender, List<ElementAndMarker> elementAndMarkers)
            => ElementAndMarkers.AddRange(elementAndMarkers);

        protected abstract List<INotifiyingObjectRenderer> LoadNotifyingObjectRenderers();

        protected virtual void LoadNonNotifyingObjectRenderers()
        {
        }

        public override void LoadDocument(FlowDocument document)
        {
            base.LoadDocument(document);
            document.Style = null;
        }

        public override object Render(MarkdownObject markdownObject)
        {
            var rendered = RenderMarkdownObject(markdownObject);
            notifyingObjectRenderers.ForEach(notifyingObjectRenderer
                => notifyingObjectRenderer.CreatedEvent -= NotifyingObjectRenderer_CreatedEvent);
            return rendered;
        }

        protected virtual object RenderMarkdownObject(MarkdownObject markdownObject)
        {
            return base.Render(markdownObject);
        }
    }
}
