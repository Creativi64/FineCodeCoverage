using System.Windows.Controls;

namespace FineCodeCoverage.Readme
{
    public partial class ReadmeControl : UserControl
    {
        private readonly IReadMeMarkdownViewModel readMeMarkdownViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadmeControl"/> class.
        /// </summary>
        public ReadmeControl(IReadMeMarkdownViewModel readMeMarkdownViewModel)
        {
            this.InitializeComponent();
            this.Viewer.Pipeline = readMeMarkdownViewModel.MarkdownPipeline;
            this.Viewer.Markdown = readMeMarkdownViewModel.MarkdownString;
            this.readMeMarkdownViewModel = readMeMarkdownViewModel;
        }
        private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            => this.readMeMarkdownViewModel.LinkClicked(e.Parameter.ToString());

        private void ClickOnImage(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            => this.readMeMarkdownViewModel.ImageClicked(e.Parameter.ToString());
    }
}