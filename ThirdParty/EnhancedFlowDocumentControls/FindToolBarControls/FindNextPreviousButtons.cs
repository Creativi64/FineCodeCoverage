using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EnhancedFlowDocumentControls.FindToolBarControls
{
    public class FindNextPreviousButtons : Control
    {
        static FindNextPreviousButtons() => DefaultStyleKeyProperty.OverrideMetadata(typeof(FindNextPreviousButtons), new FrameworkPropertyMetadata(typeof(FindNextPreviousButtons)));

        public bool ShowTooltips
        {
            get => (bool)GetValue(ShowTooltipsProperty);
            set => SetValue(ShowTooltipsProperty, value);
        }

        public static readonly DependencyProperty ShowTooltipsProperty =
            DependencyProperty.Register(nameof(ShowTooltips), typeof(bool), typeof(FindNextPreviousButtons), new PropertyMetadata(true));

        public string FindNextTooltip
        {
            get => (string)GetValue(FindNextTooltipProperty);
            set => SetValue(FindNextTooltipProperty, value);
        }

        public static readonly DependencyProperty FindNextTooltipProperty =
            DependencyProperty.Register(nameof(FindNextTooltip), typeof(string), typeof(FindNextPreviousButtons), new PropertyMetadata("Find Next"));

        public string FindPreviousTooltip
        {
            get => (string)GetValue(FindPreviousTooltipProperty);
            set => SetValue(FindPreviousTooltipProperty, value);
        }

        public static readonly DependencyProperty FindPreviousTooltipProperty =
            DependencyProperty.Register(nameof(FindPreviousTooltip), typeof(string), typeof(FindNextPreviousButtons), new PropertyMetadata("Find Previous"));

        public Brush BackgroundMouseOver
        {
            get => (Brush)GetValue(BackgroundMouseOverProperty);
            set => SetValue(BackgroundMouseOverProperty, value);
        }

        public static readonly DependencyProperty BackgroundMouseOverProperty =
            DependencyProperty.Register(nameof(BackgroundMouseOver), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush StateBackground
        {
            get => (Brush)GetValue(StateBackgroundProperty);
            set => SetValue(StateBackgroundProperty, value);
        }

        public static readonly DependencyProperty StateBackgroundProperty =
            DependencyProperty.Register(nameof(StateBackground), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush StateBorderBrush
        {
            get => (Brush)GetValue(StateBorderBrushProperty);
            set => SetValue(StateBorderBrushProperty, value);
        }

        public static readonly DependencyProperty StateBorderBrushProperty =
            DependencyProperty.Register(nameof(StateBorderBrush), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush BorderMouseOverBrush
        {
            get => (Brush)GetValue(BorderMouseOverBrushProperty);
            set => SetValue(BorderMouseOverBrushProperty, value);
        }

        public static readonly DependencyProperty BorderMouseOverBrushProperty =
            DependencyProperty.Register(nameof(BorderMouseOverBrush), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush BackgroundFocusedBrush
        {
            get => (Brush)GetValue(BackgroundFocusedBrushProperty);
            set => SetValue(BackgroundFocusedBrushProperty, value);
        }

        public static readonly DependencyProperty BackgroundFocusedBrushProperty =
            DependencyProperty.Register(nameof(BackgroundFocusedBrush), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush BorderFocusedBrush
        {
            get => (Brush)GetValue(BorderFocusedBrushProperty);
            set => SetValue(BorderFocusedBrushProperty, value);
        }

        public static readonly DependencyProperty BorderFocusedBrushProperty =
            DependencyProperty.Register(nameof(BorderFocusedBrush), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush BackgroundMouseOverFocused
        {
            get => (Brush)GetValue(BackgroundMouseOverFocusedProperty);
            set => SetValue(BackgroundMouseOverFocusedProperty, value);
        }

        public static readonly DependencyProperty BackgroundMouseOverFocusedProperty =
            DependencyProperty.Register(nameof(BackgroundMouseOverFocused), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush BorderMouseOverFocusedBrush
        {
            get => (Brush)GetValue(BorderMouseOverFocusedBrushProperty);
            set => SetValue(BorderMouseOverFocusedBrushProperty, value);
        }

        public static readonly DependencyProperty BorderMouseOverFocusedBrushProperty =
            DependencyProperty.Register(nameof(BorderMouseOverFocusedBrush), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush BackgroundPressed
        {
            get => (Brush)GetValue(BackgroundPressedProperty); set => SetValue(BackgroundPressedProperty, value);
        }

        public static readonly DependencyProperty BackgroundPressedProperty =
            DependencyProperty.Register(nameof(BackgroundPressed), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush BorderPressedBrush
        {
            get => (Brush)GetValue(BorderPressedBrushProperty);
            set => SetValue(BorderPressedBrushProperty, value);
        }

        public static readonly DependencyProperty BorderPressedBrushProperty =
            DependencyProperty.Register(nameof(BorderPressedBrush), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush GlyphBrush
        {
            get => (Brush)GetValue(GlyphBrushProperty); set => SetValue(GlyphBrushProperty, value);
        }

        public static readonly DependencyProperty GlyphBrushProperty =
            DependencyProperty.Register(nameof(GlyphBrush), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush StateGlyphBrush
        {
            get => (Brush)GetValue(StateGlyphBrushProperty);
            set => SetValue(StateGlyphBrushProperty, value);
        }

        public static readonly DependencyProperty StateGlyphBrushProperty =
            DependencyProperty.Register(nameof(StateGlyphBrush), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush GlyphMouseOverBrush
        {
            get => (Brush)GetValue(GlyphMouseOverBrushProperty); set => SetValue(GlyphMouseOverBrushProperty, value);
        }

        public static readonly DependencyProperty GlyphMouseOverBrushProperty =
            DependencyProperty.Register(nameof(GlyphMouseOverBrush), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush GlyphFocusedBrush
        {
            get => (Brush)GetValue(GlyphFocusedBrushProperty);
            set => SetValue(GlyphFocusedBrushProperty, value);
        }

        public static readonly DependencyProperty GlyphFocusedBrushProperty =
            DependencyProperty.Register(nameof(GlyphFocusedBrush), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush GlyphMouseOverFocusedBrush
        {
            get => (Brush)GetValue(GlyphMouseOverFocusedBrushProperty);
            set => SetValue(GlyphMouseOverFocusedBrushProperty, value);
        }

        public static readonly DependencyProperty GlyphMouseOverFocusedBrushProperty =
            DependencyProperty.Register(nameof(GlyphMouseOverFocusedBrush), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));

        public Brush GlyphPressedBrush
        {
            get => (Brush)GetValue(GlyphPressedBrushProperty);
            set => SetValue(GlyphPressedBrushProperty, value);
        }

        public static readonly DependencyProperty GlyphPressedBrushProperty =
            DependencyProperty.Register(nameof(GlyphPressedBrush), typeof(Brush), typeof(FindNextPreviousButtons), new PropertyMetadata(null));
    }
}
