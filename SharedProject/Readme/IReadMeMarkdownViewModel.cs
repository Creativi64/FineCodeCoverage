using System;

namespace FineCodeCoverage.Readme
{
    public interface IReadMeMarkdownViewModel
    {
        event EventHandler ReadyEvent;
        void ImageClicked(string url);

        void LinkClicked(string url);
        FlowDocumentElementMarkers FlowDocumentElementMarkers { get;}
    }
}
