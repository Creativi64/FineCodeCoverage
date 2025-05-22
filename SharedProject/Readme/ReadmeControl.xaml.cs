using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
            this.Loaded += ReadmeControl_Loaded;
            this.readMeMarkdownViewModel = readMeMarkdownViewModel;
        }

        public FlowDocument FlowDocument
        {
            get { return (FlowDocument)GetValue(FlowDocumentProperty); }
            set { SetValue(FlowDocumentProperty, value); }
        }

        public static readonly DependencyProperty FlowDocumentProperty =
            DependencyProperty.Register(nameof(FlowDocument), typeof(FlowDocument), typeof(ReadmeControl), new PropertyMetadata(null));
        private Dictionary<MarkdownTypeMarker, Style> styleDictionary;

        private void ReadmeControl_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                SetStyleDictionary();
                var (flowDocument, elementAndMarkers) = FCCMarkdownToFlowDocumentConverter.MarkdownToFlowDocument(readMeMarkdownViewModel.MarkdownString);
                SetStyles(elementAndMarkers);
                FlowDocument = flowDocument;
            }).FileAndForget("OOPS");
        }

        private void SetStyleDictionary()
        {
            styleDictionary = new Dictionary<MarkdownTypeMarker, Style>();
            Enum.GetValues(typeof(MarkdownTypeMarker)).OfType<MarkdownTypeMarker>().ToList().ForEach(marker =>
            {
                if (TryFindResource(marker) is Style style)
                {
                    styleDictionary.Add(marker, style);
                }
            });
        }

        private void SetStyles(List<ElementAndMarker> elementAndMarkers)
        {
            foreach (var elementAndMarker in elementAndMarkers)
            {
                // special consideration for images....
                var marker = elementAndMarker.Marker;
                if (marker == MarkdownTypeMarker.LinkInlineImage)
                {

                }
                else
                {
                    if (elementAndMarker.Element is DependencyObject dependencyObject)
                    {
                        SetStyle(marker, dependencyObject);
                    }
                }
            }
        }

        private void SetStyle(MarkdownTypeMarker marker, DependencyObject dependencyObject)
        {
            if (this.styleDictionary.TryGetValue(marker, out var style))
            {
                dependencyObject.SetValue(FrameworkElement.StyleProperty, style);
            }
        }

        private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            => this.readMeMarkdownViewModel.LinkClicked(e.Parameter.ToString());

        private void ClickOnImage(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            => this.readMeMarkdownViewModel.ImageClicked(e.Parameter.ToString());
    }
}