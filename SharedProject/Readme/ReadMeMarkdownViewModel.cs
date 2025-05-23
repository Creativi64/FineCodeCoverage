using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadMeMarkdownViewModel))]
    internal class ReadMeMarkdownViewModel : IReadMeMarkdownViewModel
    {
        private readonly IProcess process;
        private readonly IReadmeProvider readmeProvider;
        private readonly IReadmeToFlowDocumentService readmeToFlowDocumentService;

        public event EventHandler ReadyEvent;

        [ImportingConstructor]
        public ReadMeMarkdownViewModel(
            IProcess process,
            IReadmeProvider readmeProvider,
            IReadmeToFlowDocumentService readmeToFlowDocumentService)
        {
            this.process = process;
            this.readmeProvider = readmeProvider;
            this.readmeToFlowDocumentService = readmeToFlowDocumentService;
            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                // get off the main thread
                await TaskScheduler.Default;
                var readmeString = this.readmeProvider.GetReadme();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                FlowDocumentElementMarkers = readmeToFlowDocumentService.MarkdownToFlowDocument(readmeString);
                ReadyEvent?.Invoke(this, EventArgs.Empty);
            });
        }


        public FlowDocumentElementMarkers FlowDocumentElementMarkers { get; set; }

        private static bool IsRelativePath(string url) => Uri.IsWellFormedUriString(url, UriKind.Relative);

        public void LinkClicked(string url)
        {
            if (IsRelativePath(url))
            {
                url = "https://github.com/FortuneN/FineCodeCoverage/blob/master/" + url;
            }

            this.process.Start(url);
        }

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
    }

}
