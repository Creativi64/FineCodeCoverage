using System.Windows;
using System.Windows.Media;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Output
{
    internal sealed class DefaultLineBrushCreator : ILineBrushCreator
    {
        private static readonly VsThemeLifetimeCache<Color, Brush> s_cache = new VsThemeLifetimeCache<Color, Brush>();

        private DefaultLineBrushCreator()
        {
        }

        public Brush Create(Color penColor)
            => s_cache.GetOrAdd(penColor, () =>
            {
                const int tile = 10;
                const bool hash = false;
                var line = new LineGeometry(
                    new Point(0, tile),
                    new Point(tile, 0)
                );

                var geometryGroup = new GeometryGroup();
                geometryGroup.Children.Add(line);
                if (hash)
                {
#pragma warning disable CS0162 // Unreachable code detected
                    var line2 = new LineGeometry(
                        new Point(tile, tile),
                        new Point(0, 0)
                    );
#pragma warning restore CS0162 // Unreachable code detected
                    geometryGroup.Children.Add(line2);
                }

                var geometryDrawing = new GeometryDrawing
                {
                    Brush = Brushes.Transparent,
                    Pen = new Pen(new SolidColorBrush(penColor), 1),
                    Geometry = geometryGroup,
                };

                var brush = new DrawingBrush(geometryDrawing)
                {
                    TileMode = TileMode.Tile,
                    Viewport = new Rect(0, 0, tile, tile),
                    ViewportUnits = BrushMappingMode.Absolute,
                    Stretch = Stretch.None,
                };
                brush.Freeze();
                return brush;
            });

        public static DefaultLineBrushCreator Instance { get; } = new DefaultLineBrushCreator();
    }
}
