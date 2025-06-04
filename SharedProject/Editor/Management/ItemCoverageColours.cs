using System.Windows.Media;

namespace FineCodeCoverage.Editor.Management
{
    internal class ItemCoverageColours : IItemCoverageColours
    {
        public ItemCoverageColours(Color foreground, Color background)
        {
            Foreground = foreground;
            Background = background;
        }

        public Color Foreground { get; }

        public Color Background { get; }

        public bool Equals(IItemCoverageColours other)
            => Foreground == other.Foreground && Background == other.Background;
    }
}
