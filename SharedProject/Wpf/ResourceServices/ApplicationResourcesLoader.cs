using System.ComponentModel.Composition;
using System.Windows;

namespace FineCodeCoverage.Wpf
{
    [Export(typeof(IApplicationResourcesLoader))]
    public class ApplicationResourcesLoader : IApplicationResourcesLoader
    {
        public void AddFromExecutingAssembly(string resourcePath)
            => Application.Current.Resources.AddFromExecutingAssembly(resourcePath);
    }
}
