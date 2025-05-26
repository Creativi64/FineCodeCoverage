using System.Collections.Generic;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    public interface IReadMeMarkdownViewModel
    {
        void LinkClicked(string url);
        FlowDocument FlowDocument { get; }
        IReadOnlyList<ElementAndMarker> ElementAndMarkers { get; }
    }
}
