using System;
using System.Collections.Generic;
using Markdig.Renderers;

namespace FineCodeCoverage.Readme
{
    public interface INotifiyingObjectRenderer : IMarkdownObjectRenderer
    {
        event EventHandler<List<ElementAndMarker>> CreatedEvent;
    }
}
