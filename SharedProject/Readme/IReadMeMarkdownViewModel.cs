using System.Collections.Generic;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    public interface IReadMeMarkdownViewModel
    {
        void ImageClicked(string url);

        void LinkClicked(string url);
        FlowDocument FlowDocument { get; }
        IReadOnlyList<ElementAndMarker> ElementAndMarkers { get; }
    }
}
