using System;
using System.Windows;
using System.Windows.Media;

namespace FineCodeCoverage.Wpf
{
    public static class LineBrushCreator
    {
        public static Brush Create(SolidColorBrush penBrush, int tile = 10, int thickness = 1, int angle = 45)
        {
            return LineBrushCache.GetOrAdd(penBrush.Color, tile, thickness, angle, () =>
            {
                var group = new DrawingGroup();

                var radians = angle * Math.PI / 180.0;
                var sin = Math.Sin(radians);
                var cos = Math.Cos(radians);
                //todo

                // Draw one line across the tile
                var line = new LineGeometry(
                    new Point(0, tile),
                    new Point(tile, 0)
                );

                var geometryDrawing = new GeometryDrawing
                {
                    Brush = Brushes.Transparent,
                    Pen = new Pen(penBrush, thickness),
                    Geometry = line
                };

                group.Children.Add(geometryDrawing);

                return new DrawingBrush(group)
                {
                    TileMode = TileMode.Tile,
                    Viewport = new Rect(0, 0, tile, tile),
                    ViewportUnits = BrushMappingMode.Absolute,
                    Stretch = Stretch.None
                };
            });


        }
    }


}
