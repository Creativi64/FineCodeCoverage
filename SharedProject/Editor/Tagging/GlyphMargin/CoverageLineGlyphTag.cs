using System.Windows.Media;
using Microsoft.VisualStudio.Text.Editor;

namespace FineCodeCoverage.Editor.Tagging.GlyphMargin
{
    internal sealed class CoverageLineGlyphTag : IGlyphTag
    {
        public Color Colour { get; }

        public CoverageLineGlyphTag(Color colour) => Colour = colour;
    }
}
