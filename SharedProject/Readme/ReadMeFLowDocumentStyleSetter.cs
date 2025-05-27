using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Wpf;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadMeFlowDocumentStylesSetter))]
    internal class ReadMeFLowDocumentStyleSetter : IReadMeFlowDocumentStylesSetter
    {
        private readonly IApplicationResourcesLoader applicationResourcesLoader;

        [ImportingConstructor]
        public ReadMeFLowDocumentStyleSetter(
            IApplicationResourcesLoader applicationResourcesLoader
        )
        {
            this.applicationResourcesLoader = applicationResourcesLoader;
        }

        public void SetStyles(IReadOnlyList<ElementAndMarker> elementAndMarkers)
        {
            applicationResourcesLoader.AddFromExecutingAssembly("Readme/ReadMeResourceDictionary.xaml");
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
