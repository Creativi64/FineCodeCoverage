using FineCodeCoverage.Core.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadMeMarkdownViewModel))]
    internal class ReadMeMarkdownViewModel : IReadMeMarkdownViewModel
    {
        private readonly IProcess process;
        private readonly IReadmeProvider readmeProvider;
        private readonly IFCCMarkdownFlowDocumentProvider fccMarkdownFlowDocumentProvider;

        [ImportingConstructor]
        public ReadMeMarkdownViewModel(
            IProcess process,
            IReadmeProvider readmeProvider,
            IFCCMarkdownFlowDocumentProvider fccMarkdownFlowDocumentProvider
            )
        {
            this.process = process;
            this.readmeProvider = readmeProvider;
            this.fccMarkdownFlowDocumentProvider = fccMarkdownFlowDocumentProvider;
        }

        private FlowDocument GetFlowDocument()
        {
            var templatedReadme = this.readmeProvider.GetReadme();
            var flowDocumentElementMarkers = fccMarkdownFlowDocumentProvider.Provide(
                templatedReadme, "FCCOptionsTable"
                )();
            this.ElementAndMarkers = flowDocumentElementMarkers.ElementAndMarkers;
            return flowDocumentElementMarkers.FlowDocument;
        }

        public FlowDocument FlowDocument
        {
            get => GetFlowDocument();
        }

        public List<ElementAndMarker> ElementAndMarkers { get; private set; }

        #region clicks
        #region link clicked
        private static bool IsRelativePath(string url) => Uri.IsWellFormedUriString(url, UriKind.Relative);

        public void LinkClicked(string url)
        {
            if (IsRelativePath(url))
            {
                url = "https://github.com/FortuneN/FineCodeCoverage/blob/master/" + url;
            }

            this.process.Start(url);
        }
        #endregion

        #region image clicked
        private static bool IsYoutubeImage(string url) => url.StartsWith("https://img.youtube.com");

        public void ImageClicked(string url)
        {
            if (IsYoutubeImage(url))
            {
                this.process.Start(YoutubeImageToYoutubeVideo(url));
            }
        }

        private static string YoutubeImageToYoutubeVideo(string url)
        {
            string[] segments = url.Split('/');
            string videoId = segments[segments.Length - 2];
            return $"https://youtu.be/{videoId}";
        }
        #endregion
        #endregion
    }
}
