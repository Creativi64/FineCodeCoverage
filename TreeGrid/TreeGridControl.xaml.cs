using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace TreeGrid
{
    /// <summary>
    /// Interaction logic for TreeGrid.xaml
    /// </summary>
    public partial class TreeGridControl : UserControl
    {
        private List<string> ColumnPropertyNames { get; set; }

        #region dependency properties

        public bool BindTextBlockForeground
        {
            get { return (bool)GetValue(BindTextBlockForegroundProperty); }
            set { SetValue(BindTextBlockForegroundProperty, value); }
        }

        public static readonly DependencyProperty BindTextBlockForegroundProperty =
            DependencyProperty.Register("BindTextBlockForeground", typeof(bool), typeof(TreeGridControl), new PropertyMetadata(true));

        public Brush TreeViewBackground
        {
            get { return (Brush)GetValue(TreeViewBackgroundProperty); }
            set { SetValue(TreeViewBackgroundProperty, value); }
        }

        public static readonly DependencyProperty TreeViewBackgroundProperty =
            DependencyProperty.Register("TreeViewBackground", typeof(Brush), typeof(TreeGridControl), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public Brush TreeViewForeground
        {
            get { return (Brush)GetValue(TreeViewForegroundProperty); }
            set { SetValue(TreeViewForegroundProperty, value); }
        }

        public static readonly DependencyProperty TreeViewForegroundProperty =
            DependencyProperty.Register("TreeViewForeground", typeof(Brush), typeof(TreeGridControl), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public Brush TreeGridControlBackground
        {
            get { return (Brush)GetValue(TreeGridControlBackgroundProperty); }
            set { SetValue(TreeGridControlBackgroundProperty, value); }
        }

        public static readonly DependencyProperty TreeGridControlBackgroundProperty =
            DependencyProperty.Register("TreeGridControlBackground", typeof(Brush), typeof(TreeGridControl), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public Brush HeaderBackground
        {
            get { return (Brush)GetValue(HeaderBackgroundProperty); }
            set { SetValue(HeaderBackgroundProperty, value); }
        }

        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register("HeaderBackground", typeof(Brush), typeof(TreeGridControl), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public Brush HeaderForeground
        {
            get { return (Brush)GetValue(HeaderForegroundProperty); }
            set { SetValue(HeaderForegroundProperty, value); }
        }

        public static readonly DependencyProperty HeaderForegroundProperty =
            DependencyProperty.Register("HeaderForeground", typeof(Brush), typeof(TreeGridControl), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public Brush HeaderGridLines
        {
            get { return (Brush)GetValue(HeaderGridLinesProperty); }
            set { SetValue(HeaderGridLinesProperty, value); }
        }

        public static readonly DependencyProperty HeaderGridLinesProperty =
            DependencyProperty.Register("HeaderGridLines", typeof(Brush), typeof(TreeGridControl), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public Style HeaderStyle
        {
            get { return (Style)GetValue(HeaderStyleProperty); }
            set { SetValue(HeaderStyleProperty, value); }
        }

        public static readonly DependencyProperty HeaderStyleProperty =
            DependencyProperty.Register("HeaderStyle", typeof(Style), typeof(TreeGridControl), new PropertyMetadata(null, OnHeaderStyleChanged));

        private static void OnHeaderStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TreeGridControl)d;
            control.SetHeaderStyle();
        }

        private void SetHeaderStyle()
        {
            Style baseStyle = HeaderStyle ?? GetDefaultDataGridColumnHeaderStyle();
            GridHeader.ColumnHeaderStyle = AddRequiredDataGridColumnHeaderSetters(baseStyle); ;
        }

        private Style GetDefaultDataGridColumnHeaderStyle() => (this.Resources["DataGridColumnHeaderStyle"] as Style);

        private Style GetFocusableAndClickableColumnHeaderStyle() => (this.Resources["FocusableAndClickableColumnHeaderStyle"] as Style);

        private Style AddRequiredDataGridColumnHeaderSetters(Style baseStyle)
        {
            var focusableAndClickableStyle = GetFocusableAndClickableColumnHeaderStyle();
            Style mergedStyle = new Style(typeof(DataGridColumnHeader), baseStyle);

            foreach (SetterBase setter in focusableAndClickableStyle.Setters)
            {
                mergedStyle.Setters.Add(setter);
            }
            foreach (TriggerBase trigger in focusableAndClickableStyle.Triggers)
            {
                mergedStyle.Triggers.Add(trigger);
            }
            return mergedStyle;
        }

        public static readonly DependencyProperty HierarchicalDataTemplateProperty = DependencyProperty.Register(
            nameof(HierarchicalDataTemplate), typeof(HierarchicalDataTemplate), typeof(TreeGridControl), new PropertyMetadata(null, OnHierarchicalDataTemplateChanged));

        public HierarchicalDataTemplate HierarchicalDataTemplate
        {
            get { return (HierarchicalDataTemplate)GetValue(HierarchicalDataTemplateProperty); }
            set { SetValue(HierarchicalDataTemplateProperty, value); }
        }

        private static void OnHierarchicalDataTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TreeGridControl)d;
            control.SetHierarchicalDataTemplate();
        }

        private void SetHierarchicalDataTemplate()
        {
            var hdt = HierarchicalDataTemplate;
            if (hdt == null)
            {
                hdt = FindResource("DefaultHierarchicalDataTemplate") as HierarchicalDataTemplate;
            }
            TreeView.ItemTemplate = hdt;
        }

        public ITreeGridViewModel ViewModel
        {
            get { return (ITreeGridViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ITreeGridViewModel), typeof(TreeGridControl), new PropertyMetadata(null, OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TreeGridControl)d;
            control.Initialize();
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(TreeGridControl), new PropertyMetadata(null));

        #endregion

        private void Initialize()
        {
            InitializeColumns();
            if (ViewModel != null)
            {
                new DragBeforeFirstBlocker(ViewModel.ColumnManager, GridHeader, () => ViewModel.ColumnManager.SortColumnsArray());
            }
        }

        private void InitializeColumns()
        {
            if (ViewModel == null)
            {
                return;
            }
            GenerateGridHeaderColumns();
            var firstColumn = ViewModel.ColumnManager.Columns[0];
            firstColumn.PropertyChanged += (s, e) =>
            {
                // double clicking the divider sets to auto - which is no good for GridWidth
                // undone in DataGridColumnHeader_MouseDoubleClick
                if (e.PropertyName == "Width" && !firstColumn.Width.IsAuto)
                {
                    foreach (var treeItem in this.ViewModel.Items)
                    {
                        treeItem.AdjustWidth(firstColumn.Width.Value);
                    }
                }
            };
        }

        private IEnumerable<string> GetColumnPropertyNames()
            => ViewModel.ColumnManager.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.Instance).Where(p => p.PropertyType == typeof(ColumnData))
                .Select(p => p.Name);

        private void GenerateGridHeaderColumns()
        {
            GridHeader.Columns.Clear();

            ColumnPropertyNames = GetColumnPropertyNames().ToList();
            foreach (var columnName in ColumnPropertyNames)
            {
                GridHeader.Columns.Add(
                    CreateBoundGridHeaderColumn(columnName)
                );
            }
        }

        private DataGridTextColumn CreateBoundGridHeaderColumn(string columnPropertyName)
        {
            var column = new DataGridTextColumn();
            var ColumnManager = ViewModel.ColumnManager;

            BindingOperations.SetBinding(column, DataGridTextColumn.HeaderProperty, new Binding($"{columnPropertyName}.Name") { Source = ColumnManager });
            BindingOperations.SetBinding(column, DataGridTextColumn.DisplayIndexProperty, new Binding($"{columnPropertyName}.DisplayIndex") { Source = ColumnManager, FallbackValue = 1, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(column, DataGridTextColumn.WidthProperty, new Binding($"{columnPropertyName}.Width") { Source = ColumnManager, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(column, DataGridTextColumn.VisibilityProperty, new Binding($"{columnPropertyName}.IsVisible") { Source = ColumnManager, Mode = BindingMode.TwoWay, Converter = new BooleanToVisibilityConverter() });
            BindingOperations.SetBinding(column, DataGridTextColumn.MinWidthProperty, new Binding($"{columnPropertyName}.MinWidth") { Source = ColumnManager });
            BindingOperations.SetBinding(column, DataGridTextColumn.SortDirectionProperty, new Binding($"{columnPropertyName}.SortDirection") { Source = ColumnManager, Mode = BindingMode.TwoWay });

            if (column.DisplayIndex == 0)
            {
                column.CanUserReorder = false;
            }
            return column;
        }

        public void TreeItemContentControlLoaded(ContentPresenter contentPresenter)
        {
            var grid = contentPresenter.GetAscendant<Grid>();
            var panel = GetPanel(contentPresenter);
            SetTreeItemGridColumnDefinitions(grid);
            AddTreeItemElementsToGrid(panel, grid);
        }

        private void SetTreeItemGridColumnDefinitions(Grid grid)
        {
            for (var i = 1; i < ViewModel.ColumnManager.Columns.Length; i++)
            {
                Binding widthBinding = new Binding
                {
                    Source = ViewModel.ColumnManager,
                    Path = new PropertyPath($"Columns[{i}].GridWidth")
                };

                var columnDefinition = new ColumnDefinition();
                BindingOperations.SetBinding(columnDefinition, ColumnDefinition.WidthProperty, widthBinding);
                grid.ColumnDefinitions.Add(columnDefinition);

            }
        }

        private void AddTreeItemElementsToGrid(Panel panel, Grid grid)
        {
            grid.Children.Remove(this);

            var numChildren = VisualTreeHelper.GetChildrenCount(panel);
            for (int i = 0; i < numChildren; i++)
            {
                var child = Reparent(panel, grid);
                BindTreeItem(child, i);
            }
        }

        private void BindTreeItem(UIElement child, int index)
        {
            string columnPropertyName = null;
            if (child is FrameworkElement frameworkElement && frameworkElement.Tag is string tag)
            {
                columnPropertyName = tag;
            }
            var displayIndexBinding = new Binding
            {
                Source = this,
                Path = GetColumnManagerPropertyPath("DisplayIndex", index, columnPropertyName)
            };

            BindingOperations.SetBinding(child, Grid.ColumnProperty, displayIndexBinding);
            if (index != 0)
            {
                var visibilityBinding = new Binding
                {
                    Source = this,
                    Path = GetColumnManagerPropertyPath("IsVisible", index, columnPropertyName),
                    Converter = new BooleanToVisibilityConverter()
                };

                BindingOperations.SetBinding(child, UIElement.VisibilityProperty, visibilityBinding);
            }

            if (child is TextBlock textBlock && this.BindTextBlockForeground)
            {
                var foregroundBinding = new Binding("Foreground");

                BindingOperations.SetBinding(textBlock, TextBlock.ForegroundProperty, foregroundBinding);
            }
        }

        private PropertyPath GetColumnManagerPropertyPath(string property, string columnPropertyName)
        {
            return new PropertyPath($"ViewModel.ColumnManager.{columnPropertyName}.{property}");
        }

        private PropertyPath GetColumnManagerPropertyPath(string property, int index)
        {
            return new PropertyPath($"ViewModel.ColumnManager.{this.ColumnPropertyNames[index]}.{property}");
        }

        private PropertyPath GetColumnManagerPropertyPath(string property, int index, string columnPropertyName)
        {
            if (columnPropertyName != null)
            {
                return GetColumnManagerPropertyPath(property, columnPropertyName);
            }
            return GetColumnManagerPropertyPath(property, index);
        }

        private UIElement Reparent(Panel panel, Grid grid)
        {
            var child = VisualTreeHelper.GetChild(panel, 0) as UIElement;
            panel.Children.Remove(child);
            grid.Children.Add(child);
            return child;
        }

        private Panel GetPanel(ContentPresenter contentPresenter) => VisualTreeHelper.GetChild(contentPresenter, 0) as Panel;

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel != null && sender is TreeViewItem treeViewItem)
            {
                var treeItem = treeViewItem.DataContext as ITreeItem;
                if (!treeItem.Children.Any())
                {
                    ViewModel.LeafTreeItemDoubleClick(treeItem);
                }
            }
        }

        private void DataGridColumnHeader_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Down)
                return;
            e.Handled = true;
            this.TreeView.Focus();
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);
            if (!(this.TreeView.SelectedItem is ITreeItem selectedItem))
                return;
            selectedItem.IsSelectionActive = (bool)e.NewValue;
        }

        private void TreeView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.GridHeaderScrollViewer.Width = e.ViewportWidth;
            if (e.HorizontalChange == 0.0)
                return;
            this.GridHeaderScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
        }

        private void TreeViewItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) => e.Handled = true;

        public TreeGridControl()
        {
            InitializeComponent();
            SetHierarchicalDataTemplate();
            SetHeaderStyle();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedTreeViewItem = (ITreeItem)e.NewValue;
            }
        }

        private void DataGridColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (sender is DataGridColumnHeader gridColumnHeader)
            {
                ViewModel.Sort(gridColumnHeader.DisplayIndex);
            }
        }

        private void DataGridColumnHeader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel != null)
            {
                foreach (var dataColumn in GridHeader.Columns)
                {
                    var columnData = ViewModel.ColumnManager.Columns.First(c => c.DisplayIndex == dataColumn.DisplayIndex);
                    // double clicking the divider sets to auto - which is no good for GridWidth
                    if (columnData.Width.IsAuto)
                    {
                        dataColumn.Width = dataColumn.MinWidth;
                    }
                }
            }

        }

        private void TreeView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Up || !(this.TreeView.SelectedItem is ITreeItem treeItem && treeItem.Parent == null))
                return;
            e.Handled = true;
            if (ViewModel != null)
            {
                var firstHeader = GetFirstHeader(this.GridHeader);
                firstHeader?.Focus();
            }
        }

        private static DataGridColumnHeader GetFirstHeader(
            DependencyObject reference
        )
        {
            for (int childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(reference); ++childIndex)
            {
                DependencyObject child = VisualTreeHelper.GetChild(reference, childIndex);
                if (child is DataGridColumnHeader header1 && header1.Column is DataGridColumn col && col.DisplayIndex == 0)
                    return header1;
                DataGridColumnHeader header2 = GetFirstHeader(child);
                if (header2 != null)
                    return header2;
            }
            return null;
        }
    }
}
