using System.ComponentModel.Composition;
using System.Windows;

namespace FineCodeCoverage.Wpf
{
    [Export(typeof(IApplicationResourcesLoader))]
    public class ApplicationResourcesLoader : IApplicationResourcesLoader
    {
        public void AddFromExecutingAssembly(string path)
        {
            Application.Current.Resources.AddFromExecutingAssembly(path);
        }
    }
}
