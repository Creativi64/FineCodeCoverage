using Markdig.Renderers;

namespace MarkdigExtended.NotifyingWpfRenderers.Base
{
    public interface INotifiyingObjectRenderer : IMarkdownObjectRenderer
    {
        event EventHandler<List<ElementAndMarker>> CreatedEvent;
    }
}
