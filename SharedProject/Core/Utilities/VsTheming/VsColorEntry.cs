namespace FineCodeCoverage.Core.Utilities
{
    public class VsColorEntry
    {
        public VsColorEntry(object iVsColorEntry, ColorName colorName)
        {
            dynamic d = iVsColorEntry as dynamic;
            this.BackgroundType = d.BackgroundType;
            this.ForegroundType = d.ForegroundType;
            this.Background = d.Background;
            this.Foreground = d.Foreground;
            this.BackgroundSource = d.BackgroundSource;
            this.ForegroundSource = d.ForegroundSource;
            this.ColorName = colorName;
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
