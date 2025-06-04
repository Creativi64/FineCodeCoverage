namespace FineCodeCoverage.Core.Utilities
{
    public class VsColorEntry
    {
        public VsColorEntry(object iVsColorEntry, ColorName colorName)
        {
            dynamic d = iVsColorEntry as dynamic;
            BackgroundType = d.BackgroundType;
            ForegroundType = d.ForegroundType;
            Background = d.Background;
            Foreground = d.Foreground;
            BackgroundSource = d.BackgroundSource;
            ForegroundSource = d.ForegroundSource;
            ColorName = colorName;
        }

        ColorName ColorName { get; }

        byte BackgroundType { get; }

        byte ForegroundType { get; }

        uint Background { get; }

        uint Foreground { get; }

        uint BackgroundSource { get; }

        uint ForegroundSource { get; }
    }
}
