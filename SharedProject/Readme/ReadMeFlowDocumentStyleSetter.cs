using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Wpf;
using MarkdigExtended.NotifyingWpfRenderers.Base;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadMeFlowDocumentStylesSetter))]
    internal sealed class ReadMeFlowDocumentStyleSetter : IReadMeFlowDocumentStylesSetter
    {
        private readonly IApplicationResourcesLoader _applicationResourcesLoader;

        [ImportingConstructor]
        public ReadMeFlowDocumentStyleSetter(
            IApplicationResourcesLoader applicationResourcesLoader) => _applicationResourcesLoader = applicationResourcesLoader;

        public static object ReadMeCodeInlineBasedOnStyleKey { get; } = new object();

        public void SetStyles(IReadOnlyList<ElementAndMarker> elementAndMarkers)
        {
            _applicationResourcesLoader.AddFromExecutingAssembly("Readme/ReadMeResourceDictionary.xaml");
            _applicationResourcesLoader.AddFromExecutingAssembly("Readme/VersionedReadMeResourceDictionary.xaml");
            elementAndMarkers.ForEach(SetStyle);
        }

        private void SetStyle(ElementAndMarker elementAndMarker)
        {
            object element = elementAndMarker.Element;
            if (element is FrameworkContentElement fce)
            {
                fce.SetResourceReference(FrameworkElement.StyleProperty, elementAndMarker.Marker);
            }
            else if (element is FrameworkElement fe)
            {
                fe.SetResourceReference(FrameworkElement.StyleProperty, elementAndMarker.Marker);
            }
        }
    }
}
