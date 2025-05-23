using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
namespace FineCodeCoverage.Readme
{
    internal static class EnumStyleResourceHelper {
        public static Dictionary<TEnum,Style> GetEnumKeyedStyles<TEnum>(this FrameworkElement frameworkElement) where TEnum : Enum
        {
            var styleDictionary = new Dictionary<TEnum, Style>();
            Enum.GetValues(typeof(TEnum)).OfType<TEnum>().ToList().ForEach(enumValue =>
            {
                if (frameworkElement.TryFindResource(enumValue) is Style style)
                {
                    styleDictionary.Add(enumValue, style);
                }
            });
            return styleDictionary;
        }
    }

    public partial class ReadmeControl : UserControl
    {
        private readonly Dictionary<MarkdownTypeMarker, Style> styleDictionary;
        private readonly IReadMeMarkdownViewModel readMeMarkdownViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadmeControl"/> class.
        /// </summary>
        public ReadmeControl(IReadMeMarkdownViewModel readMeMarkdownViewModel)
        {
            this.readMeMarkdownViewModel = readMeMarkdownViewModel;
            this.InitializeComponent();
            this.styleDictionary = this.GetEnumKeyedStyles<MarkdownTypeMarker>();
            if(readMeMarkdownViewModel.FlowDocumentElementMarkers != null)
            {
                SetFlowDocument();
            }
            else
            {
                readMeMarkdownViewModel.ReadyEvent += (s, e) => SetFlowDocument();
            }
        }

        private void SetFlowDocument()
        {
            SetStyles(this.readMeMarkdownViewModel.FlowDocumentElementMarkers.ElementAndMarkers);
            var flowDocument = this.readMeMarkdownViewModel.FlowDocumentElementMarkers.FlowDocument;
            SetStyle(new ElementAndMarker(flowDocument, MarkdownTypeMarker.FlowDocument));
            FlowDocument = flowDocument;
        }

        public FlowDocument FlowDocument
        {
            get { return (FlowDocument)GetValue(FlowDocumentProperty); }
            set { SetValue(FlowDocumentProperty, value); }
        }

        public static readonly DependencyProperty FlowDocumentProperty =
            DependencyProperty.Register(nameof(FlowDocument), typeof(FlowDocument), typeof(ReadmeControl), new PropertyMetadata(null));

        private void SetStyles(List<ElementAndMarker> elementAndMarkers) => elementAndMarkers.ForEach(SetStyle);

        private void SetStyle(ElementAndMarker elementAndMarker)
        {
            if (this.styleDictionary.TryGetValue(elementAndMarker.Marker, out var style))
            {
                var element = elementAndMarker.Element;
                if (element is FrameworkContentElement fce)
                {
                    fce.Style = style;
                }
                else if(element is FrameworkElement fe)
                {
                    fe.Style = style;
                }
            }
        }

        private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            => this.readMeMarkdownViewModel.LinkClicked(e.Parameter.ToString());

        private void ClickOnImage(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            => this.readMeMarkdownViewModel.ImageClicked(e.Parameter.ToString());
    }
}