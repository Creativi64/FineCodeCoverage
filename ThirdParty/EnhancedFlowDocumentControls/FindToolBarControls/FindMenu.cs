using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EnhancedFlowDocumentControls.FindToolBarControls
{
    public class FindMenu : Control
    {
        static FindMenu() => DefaultStyleKeyProperty.OverrideMetadata(typeof(FindMenu), new FrameworkPropertyMetadata(typeof(FindMenu)));

        public Brush MenuBackground
        {
            get => (Brush)GetValue(MenuBackgroundProperty);
            set => SetValue(MenuBackgroundProperty, value);
        }

        public static readonly DependencyProperty MenuBackgroundProperty =
            DependencyProperty.Register(nameof(MenuBackground), typeof(Brush), typeof(FindMenu));

        public Brush MenuBorderBrush
        {
            get => (Brush)GetValue(MenuBorderBrushProperty);
            set => SetValue(MenuBorderBrushProperty, value);
        }

        public static readonly DependencyProperty MenuBorderBrushProperty =
            DependencyProperty.Register(nameof(MenuBorderBrush), typeof(Brush), typeof(FindMenu));

        #region Selected menu item
        public Brush SelectedGlyphBrush
        {
            get => (Brush)GetValue(SelectedGlyphBrushProperty);
            set => SetValue(SelectedGlyphBrushProperty, value);
        }

        public static readonly DependencyProperty SelectedGlyphBrushProperty =
            DependencyProperty.Register(nameof(SelectedGlyphBrush), typeof(Brush), typeof(FindMenu));

        public Brush SelectedGlyphBackground
        {
            get => (Brush)GetValue(SelectedGlyphBackgroundProperty);
            set => SetValue(SelectedGlyphBackgroundProperty, value);
        }

        public static readonly DependencyProperty SelectedGlyphBackgroundProperty =
            DependencyProperty.Register(nameof(SelectedGlyphBackground), typeof(Brush), typeof(FindMenu));

        public Brush SelectedGlyphBorderBrush
        {
            get => (Brush)GetValue(SelectedGlyphBorderBrushProperty);
            set => SetValue(SelectedGlyphBorderBrushProperty, value);
        }

        public static readonly DependencyProperty SelectedGlyphBorderBrushProperty =
            DependencyProperty.Register(nameof(SelectedGlyphBorderBrush), typeof(Brush), typeof(FindMenu));
        #endregion

        #region Menu item
        public Brush MenuItemBackground
        {
            get => (Brush)GetValue(MenuItemBackgroundProperty);
            set => SetValue(MenuItemBackgroundProperty, value);
        }

        public static readonly DependencyProperty MenuItemBackgroundProperty =
            DependencyProperty.Register(nameof(MenuItemBackground), typeof(Brush), typeof(FindMenu));

        public Brush MenuItemHighlightedBackground
        {
            get => (Brush)GetValue(MenuItemHighlightedBackgroundProperty); set => SetValue(MenuItemHighlightedBackgroundProperty, value);
        }

        public static readonly DependencyProperty MenuItemHighlightedBackgroundProperty =
            DependencyProperty.Register(nameof(MenuItemHighlightedBackground), typeof(Brush), typeof(FindMenu));

        public Brush MenuItemForeground
        {
            get => (Brush)GetValue(MenuItemForegroundProperty);
            set => SetValue(MenuItemForegroundProperty, value);
        }

        public static readonly DependencyProperty MenuItemForegroundProperty =
            DependencyProperty.Register(nameof(MenuItemForeground), typeof(Brush), typeof(FindMenu));

        public Brush MenuItemHighlightedForeground
        {
            get => (Brush)GetValue(MenuItemHighlightedForegroundProperty);
            set => SetValue(MenuItemHighlightedForegroundProperty, value);
        }

        public static readonly DependencyProperty MenuItemHighlightedForegroundProperty =
            DependencyProperty.Register(nameof(MenuItemHighlightedForeground), typeof(Brush), typeof(FindMenu));

        public Brush MenuItemBorderBrush
        {
            get => (Brush)GetValue(MenuItemBorderBrushProperty);
            set => SetValue(MenuItemBorderBrushProperty, value);
        }

        public static readonly DependencyProperty MenuItemBorderBrushProperty =
            DependencyProperty.Register(nameof(MenuItemBorderBrush), typeof(Brush), typeof(FindMenu));

        public Brush MenuItemHighlightedBorderBrush
        {
            get => (Brush)GetValue(MenuItemHighlightedBorderBrushProperty);
            set => SetValue(MenuItemHighlightedBorderBrushProperty, value);
        }

        public static readonly DependencyProperty MenuItemHighlightedBorderBrushProperty =
            DependencyProperty.Register(nameof(MenuItemHighlightedBorderBrush), typeof(Brush), typeof(FindMenu));
        #endregion

        #region Drop down
        public Brush DropDownGlyphBrush
        {
            get => (Brush)GetValue(DropDownGlyphBrushProperty);
            set => SetValue(DropDownGlyphBrushProperty, value);
        }

        public static readonly DependencyProperty DropDownGlyphBrushProperty =
            DependencyProperty.Register(nameof(DropDownGlyphBrush), typeof(Brush), typeof(FindMenu));

        public Brush DropDownGlyphMouseOverBrush
        {
            get => (Brush)GetValue(DropDownGlyphMouseOverBrushProperty);
            set => SetValue(DropDownGlyphMouseOverBrushProperty, value);
        }

        public static readonly DependencyProperty DropDownGlyphMouseOverBrushProperty =
            DependencyProperty.Register(nameof(DropDownGlyphMouseOverBrush), typeof(Brush), typeof(FindMenu));

        public Brush DropDownGlyphFocusedBrush
        {
            get => (Brush)GetValue(DropDownGlyphFocusedBrushProperty);
            set => SetValue(DropDownGlyphFocusedBrushProperty, value);
        }

        public static readonly DependencyProperty DropDownGlyphFocusedBrushProperty =
            DependencyProperty.Register(nameof(DropDownGlyphFocusedBrush), typeof(Brush), typeof(FindMenu));

        public Brush DropDownGlyphMouseOverFocusedBrush
        {
            get => (Brush)GetValue(DropDownGlyphMouseOverFocusedBrushProperty);
            set => SetValue(DropDownGlyphMouseOverFocusedBrushProperty, value);
        }

        public static readonly DependencyProperty DropDownGlyphMouseOverFocusedBrushProperty =
            DependencyProperty.Register(nameof(DropDownGlyphMouseOverFocusedBrush), typeof(Brush), typeof(FindMenu));

        public Brush DropDownGlyphPressedBrush
        {
            get => (Brush)GetValue(DropDownGlyphPressedBrushProperty); set => SetValue(DropDownGlyphPressedBrushProperty, value);
        }

        public static readonly DependencyProperty DropDownGlyphPressedBrushProperty =
            DependencyProperty.Register(nameof(DropDownGlyphPressedBrush), typeof(Brush), typeof(FindMenu));

        public Brush DropDownStateGlyphBrush
        {
            get => (Brush)GetValue(DropDownStateGlyphBrushProperty);
            set => SetValue(DropDownStateGlyphBrushProperty, value);
        }

        public static readonly DependencyProperty DropDownStateGlyphBrushProperty =
            DependencyProperty.Register(nameof(DropDownStateGlyphBrush), typeof(Brush), typeof(FindMenu));

        public Brush DropDownGlyphDisabledBrush
        {
            get => (Brush)GetValue(DropDownGlyphDisabledBrushProperty); set => SetValue(DropDownGlyphDisabledBrushProperty, value);
        }

        public static readonly DependencyProperty DropDownGlyphDisabledBrushProperty =
            DependencyProperty.Register(nameof(DropDownGlyphDisabledBrush), typeof(Brush), typeof(FindMenu));

        /// <summary>
        /// Gets or sets fallback Brush for IsMouseOver, IsKeyboardFocused.
        /// </summary>
        public Brush DropDownStateBackground
        {
            get => (Brush)GetValue(DropDownStateBackgroundProperty);
            set => SetValue(DropDownStateBackgroundProperty, value);
        }

        public static readonly DependencyProperty DropDownStateBackgroundProperty =
            DependencyProperty.Register(nameof(DropDownStateBackground), typeof(Brush), typeof(FindMenu));

        public Brush DropDownBackgroundPressed
        {
            get => (Brush)GetValue(DropDownBackgroundPressedProperty);
            set => SetValue(DropDownBackgroundPressedProperty, value);
        }

        public static readonly DependencyProperty DropDownBackgroundPressedProperty =
            DependencyProperty.Register(nameof(DropDownBackgroundPressed), typeof(Brush), typeof(FindMenu));

        public Brush DropDownBackgroundMouseOver
        {
            get => (Brush)GetValue(DropDownBackgroundMouseOverProperty);
            set => SetValue(DropDownBackgroundMouseOverProperty, value);
        }

        public static readonly DependencyProperty DropDownBackgroundMouseOverProperty =
            DependencyProperty.Register(nameof(DropDownBackgroundMouseOver), typeof(Brush), typeof(FindMenu));

        public Brush DropDownBackgroundFocused
        {
            get => (Brush)GetValue(DropDownBackgroundFocusedProperty);
            set => SetValue(DropDownBackgroundFocusedProperty, value);
        }

        public static readonly DependencyProperty DropDownBackgroundFocusedProperty =
            DependencyProperty.Register(nameof(DropDownBackgroundFocused), typeof(Brush), typeof(FindMenu));

        public Brush DropDownBackgroundMouseOverFocused
        {
            get => (Brush)GetValue(DropDownBackgroundMouseOverFocusedProperty);
            set => SetValue(DropDownBackgroundMouseOverFocusedProperty, value);
        }

        public static readonly DependencyProperty DropDownBackgroundMouseOverFocusedProperty =
            DependencyProperty.Register(nameof(DropDownBackgroundMouseOverFocused), typeof(Brush), typeof(FindMenu));

        public Brush DropDownStateBorderBrush
        {
            get => (Brush)GetValue(DropDownStateBorderBrushProperty);
            set => SetValue(DropDownStateBorderBrushProperty, value);
        }

        public static readonly DependencyProperty DropDownStateBorderBrushProperty =
            DependencyProperty.Register(nameof(DropDownStateBorderBrush), typeof(Brush), typeof(FindMenu));

        public Brush DropDownBorderPressedBrush
        {
            get => (Brush)GetValue(DropDownBorderPressedBrushProperty);
            set => SetValue(DropDownBorderPressedBrushProperty, value);
        }

        public static readonly DependencyProperty DropDownBorderPressedBrushProperty =
            DependencyProperty.Register(nameof(DropDownBorderPressedBrush), typeof(Brush), typeof(FindMenu));

        public Brush DropDownBorderMouseOverBrush
        {
            get => (Brush)GetValue(DropDownBorderMouseOverBrushProperty);
            set => SetValue(DropDownBorderMouseOverBrushProperty, value);
        }

        public static readonly DependencyProperty DropDownBorderMouseOverBrushProperty =
            DependencyProperty.Register(nameof(DropDownBorderMouseOverBrush), typeof(Brush), typeof(FindMenu));

        public Brush DropDownBorderFocusedBrush
        {
            get => (Brush)GetValue(DropDownBorderFocusedBrushProperty);
            set => SetValue(DropDownBorderFocusedBrushProperty, value);
        }

        public static readonly DependencyProperty DropDownBorderFocusedBrushProperty =
            DependencyProperty.Register(nameof(DropDownBorderFocusedBrush), typeof(Brush), typeof(FindMenu));

        public Style DropDownFocusVisualStyle
        {
            get => (Style)GetValue(DropDownFocusVisualStyleProperty);
            set => SetValue(DropDownFocusVisualStyleProperty, value);
        }

        public static readonly DependencyProperty DropDownFocusVisualStyleProperty =
            DependencyProperty.Register(nameof(DropDownFocusVisualStyle), typeof(Style), typeof(FindMenu), new PropertyMetadata(null));

        public Brush DropDownBorderMouseOverFocusedBrush
        {
            get => (Brush)GetValue(DropDownBorderMouseOverFocusedBrushProperty);
            set => SetValue(DropDownBorderMouseOverFocusedBrushProperty, value);
        }

        public static readonly DependencyProperty DropDownBorderMouseOverFocusedBrushProperty =
            DependencyProperty.Register(nameof(DropDownBorderMouseOverFocusedBrush), typeof(Brush), typeof(FindMenu));

        #region Tooltip
        public string DropDownTooltip
        {
            get => (string)GetValue(DropDownTooltipProperty);
            set => SetValue(DropDownTooltipProperty, value);
        }

        public static readonly DependencyProperty DropDownTooltipProperty =
            DependencyProperty.Register(nameof(DropDownTooltip), typeof(string), typeof(FindMenu), new PropertyMetadata("Find..."));

        public bool ShowDropDownTooltip
        {
            get => (bool)GetValue(ShowDropDownTooltipProperty); set => SetValue(ShowDropDownTooltipProperty, value);
        }

        public static readonly DependencyProperty ShowDropDownTooltipProperty =
            DependencyProperty.Register(nameof(ShowDropDownTooltip), typeof(bool), typeof(FindMenu), new PropertyMetadata(true));
        #endregion

        #endregion
    }
}
