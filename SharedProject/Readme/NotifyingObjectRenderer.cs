using Markdig.Renderers.Wpf;
using Markdig.Renderers;
using Markdig.Syntax;
using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Readme
{
    public class ElementAndMarker
    {
        public ElementAndMarker(object element, MarkdownTypeMarker marker)
        {
            this.Element = element;
            this.Marker = marker;
        }

        public object Element { get; }
        public MarkdownTypeMarker Marker { get; }
    }
    public interface INotifiyingObjectRenderer : IMarkdownObjectRenderer
    {
        event EventHandler<List<ElementAndMarker>> CreatedEvent;
    }

    public class NotifyingObjectRenderer<TObject> :  WpfObjectRenderer<TObject>, INotifiyingObjectRenderer where TObject : MarkdownObject
    {
        public event EventHandler<List<ElementAndMarker>> CreatedEvent;
        protected sealed override void Write(WpfRenderer renderer, TObject obj)
        {
            List<ElementAndMarker> elementMarkers = null;
            var element = WriteAndReturn(renderer, obj);
            if (element != null)
            {
                elementMarkers = new List<ElementAndMarker> { element };
            }
            else
            {
                elementMarkers = WriteAndReturns(renderer, obj);
            }
            if (elementMarkers == null)
            {
                return;
            }
            CreatedEvent?.Invoke(this, elementMarkers);
        }

        protected virtual ElementAndMarker WriteAndReturn(WpfRenderer renderer, TObject obj)
        {
            return null;
        }

        protected virtual List<ElementAndMarker> WriteAndReturns(WpfRenderer renderer, TObject obj)
        {
            return null;
        }
    }
}
