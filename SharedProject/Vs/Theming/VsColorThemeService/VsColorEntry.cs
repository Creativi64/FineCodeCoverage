namespace FineCodeCoverage.Vs.Theming.VsColorThemeService
{
    public class VsColorEntry
    {
        public VsColorEntry(object iVsColorEntry, ColorName colorName)
        {
            dynamic d = iVsColorEntry;
            BackgroundType = d.BackgroundType;
            ForegroundType = d.ForegroundType;
            Background = d.Background;
            Foreground = d.Foreground;
            BackgroundSource = d.BackgroundSource;
            ForegroundSource = d.ForegroundSource;
            ColorName = colorName;
        }

        public ColorName ColorName { get; }

        public byte BackgroundType { get; }

        public byte ForegroundType { get; }

        public uint Background { get; }

        public uint Foreground { get; }

        public uint BackgroundSource { get; }

        public uint ForegroundSource { get; }
    }
}
