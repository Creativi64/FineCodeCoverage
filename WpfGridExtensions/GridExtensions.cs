using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WpfGridExtensions
{
    public class PreviousRowWrapper : RowDefinition
    {
        public static readonly DependencyProperty DefaultMarginBottomProperty =
    DependencyProperty.RegisterAttached(
        "SpacerHeight",
        typeof(double),
        typeof(PreviousRowWrapper),
        new FrameworkPropertyMetadata(8.0, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetDefaultMarginBottom(DependencyObject element, double value)
            => element.SetValue(DefaultMarginBottomProperty, value);

        public static double GetDefaultMarginBottom(DependencyObject element)
            => (double)element.GetValue(DefaultMarginBottomProperty);


        public static readonly DependencyProperty VisibleProperty =
            DependencyProperty.Register(
                nameof(Visible),
                typeof(bool),
                typeof(PreviousRowWrapper),
                new PropertyMetadata(true));

        public bool Visible
        {
            get => (bool)GetValue(VisibleProperty);
            set => SetValue(VisibleProperty, value);
        }

        public static readonly DependencyProperty MarginBottomProperty =
            DependencyProperty.Register(
                nameof(MarginBottom),
                typeof(double?),
                typeof(PreviousRowWrapper),
                new FrameworkPropertyMetadata(null));

        public double? MarginBottom
        {
            get => (double?)GetValue(MarginBottomProperty);
            set => SetValue(MarginBottomProperty, value);
        }

        public static readonly DependencyProperty RowHeightProperty =
            DependencyProperty.Register(
                nameof(RowHeight),
                typeof(GridLength),
                typeof(PreviousRowWrapper),
                new PropertyMetadata(new GridLength(1, GridUnitType.Auto)));

        public GridLength RowHeight
        {
            get => (GridLength)GetValue(RowHeightProperty);
            set => SetValue(RowHeightProperty, value);
        }



        public override void EndInit()
        {
            base.EndInit();
            ApplyBindings(this);
        }

        private static void ApplyBindings(PreviousRowWrapper spacer)
        {
            if (!(LogicalTreeHelper.GetParent(spacer) is Grid grid))
            {
                return;
            }

            int index = grid.RowDefinitions.IndexOf(spacer);
            if (index <= 0)
            {
                return;
            }

            RowDefinition controlled = grid.RowDefinitions[index - 1];
            BindControlled(spacer, controlled);
            BindSpacerHeight(spacer, grid);
        }

        private static void BindControlled(PreviousRowWrapper spacer, RowDefinition controlled)
        {
            var previousRowDefinitionHeightBinding = new MultiBinding
            {
                Mode = BindingMode.OneWay,
                Converter = PreviousRowHeightConverter.Instance,
            };

            previousRowDefinitionHeightBinding.Bindings.Add(new Binding(nameof(Visible)) { Source = spacer });
            previousRowDefinitionHeightBinding.Bindings.Add(new Binding(nameof(RowHeight)) { Source = spacer });

            _ = BindingOperations.SetBinding(controlled, RowDefinition.HeightProperty, previousRowDefinitionHeightBinding);
        }

        private static void BindSpacerHeight(PreviousRowWrapper spacer, Grid grid)
        {
            var spacerHeightBinding = new MultiBinding
            {
                Mode = BindingMode.OneWay,
                Converter = MarginBottomConverter.Instance,
            };

            spacerHeightBinding.Bindings.Add(new Binding(nameof(Visible)) { Source = spacer });
            spacerHeightBinding.Bindings.Add(new Binding(nameof(MarginBottom)) { Source = spacer });
            spacerHeightBinding.Bindings.Add(new Binding
            {
                Source = grid,
                Path = new PropertyPath(PreviousRowWrapper.DefaultMarginBottomProperty),
            });

            _ = BindingOperations.SetBinding(spacer, RowDefinition.HeightProperty, spacerHeightBinding);
        }
    }
}
