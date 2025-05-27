using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Readme
{
    public interface INotifiyingObjectRenderer : IMarkdownObjectRenderer
    {
        event EventHandler<List<ElementAndMarker>> CreatedEvent;
    }
}
