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
        private readonly IApplicationResourcesLoader _applicationResourcesLoader;

        [ImportingConstructor]
        public ReadMeFlowDocumentStyleSetter(
            IApplicationResourcesLoader applicationResourcesLoader
        ) => this._applicationResourcesLoader = applicationResourcesLoader;

        public static object ReadMeCodeInlineBasedOnStyleKey { get; } = new object();

        public void SetStyles(IReadOnlyList<ElementAndMarker> elementAndMarkers)
        {
            this._applicationResourcesLoader.AddFromExecutingAssembly("Readme/ReadMeResourceDictionary.xaml");
            this._applicationResourcesLoader.AddFromExecutingAssembly("Readme/VersionedReadMeResourceDictionary.xaml");
            elementAndMarkers.ForEach(this.SetStyle);
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
