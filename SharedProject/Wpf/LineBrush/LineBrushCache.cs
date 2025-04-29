using System;
using System.Windows.Media;

namespace FineCodeCoverage.Wpf
{
    public static class LineBrushCache
    {
        private static WeakReference<Brush> _lastBrush;
        private static Color _lastColor;
        private static int _lastTile;
        private static int _lastThickness;
        private static int _lastAngle;
        private static bool _hasCreated;

        public static Brush GetOrAdd(Color lastColor, int tile, int thickness, int angle, Func<Brush> brushCreator)
        {
            if (_hasCreated && _lastBrush.TryGetTarget(out var lastBrush))
            {
                if (_lastColor == lastColor && _lastTile == tile && _lastThickness == thickness && _lastAngle == angle)
                {
                    return lastBrush;
                }
            }
            var brush = brushCreator();
            brush.Freeze();
            _hasCreated = true;
            _lastBrush = new WeakReference<Brush>(brush);
            _lastColor = lastColor;
            _lastTile = tile;
            _lastThickness = thickness;
            _lastAngle = angle;
            return brush;
        }
    }
}
