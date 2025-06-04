using System;
using System.Collections.Generic;
using Markdig.Renderers;
using Markdig.Renderers.Wpf;
using Markdig.Syntax;

namespace FineCodeCoverage.Readme
{
    public class NotifyingObjectRenderer<TObject> : WpfObjectRenderer<TObject>, INotifiyingObjectRenderer where TObject : MarkdownObject
    {
        public event EventHandler<List<ElementAndMarker>> CreatedEvent;

        protected sealed override void Write(WpfRenderer renderer, TObject obj)
        {
            ElementAndMarker element = WriteAndReturn(renderer, obj);
            List<ElementAndMarker> elementMarkers = element != null ?
                new List<ElementAndMarker> { element } : WriteAndReturns(renderer, obj);
            if (elementMarkers == null)
            {
                return;
            }

            CreatedEvent?.Invoke(this, elementMarkers);
        }

        protected virtual ElementAndMarker WriteAndReturn(WpfRenderer renderer, TObject obj) => null;

        protected virtual List<ElementAndMarker> WriteAndReturns(WpfRenderer renderer, TObject obj) => null;
    }
}
