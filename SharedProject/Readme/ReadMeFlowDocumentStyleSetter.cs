using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Wpf;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadMeFlowDocumentStylesSetter))]
    internal class ReadMeFlowDocumentStyleSetter : IReadMeFlowDocumentStylesSetter
    {
        private readonly IApplicationResourcesLoader applicationResourcesLoader;

        [ImportingConstructor]
        public ReadMeFlowDocumentStyleSetter(
            IApplicationResourcesLoader applicationResourcesLoader
        )
        {
            this.applicationResourcesLoader = applicationResourcesLoader;
        }

        public static object ReadMeCodeInlineBasedOnStyleKey { get; } = new object();

        public void SetStyles(IReadOnlyList<ElementAndMarker> elementAndMarkers)
        {
            applicationResourcesLoader.AddFromExecutingAssembly("Readme/ReadMeResourceDictionary.xaml");
            applicationResourcesLoader.AddFromExecutingAssembly("Readme/VersionedReadMeResourceDictionary.xaml");
            elementAndMarkers.ForEach(SetStyle);
        }

        private void SetStyle(ElementAndMarker elementAndMarker)
        {
            var element = elementAndMarker.Element;
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
