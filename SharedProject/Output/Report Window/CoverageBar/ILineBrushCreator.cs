using System.Windows.Media;

namespace FineCodeCoverage.Output
{
    public interface ILineBrushCreator
    {
        Brush Create(Color penColor);
    }
}