using Markdig;

namespace FineCodeCoverage.Readme
{
    public interface IReadMeMarkdownViewModel
    {
        void ImageClicked(string url);

        void LinkClicked(string url);

        string MarkdownString { get; }

        MarkdownPipeline MarkdownPipeline { get; }
    }
}
