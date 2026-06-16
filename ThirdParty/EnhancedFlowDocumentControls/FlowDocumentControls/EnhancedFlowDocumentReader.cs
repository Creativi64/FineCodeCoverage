using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnhancedFlowDocumentControls.Management;

namespace EnhancedFlowDocumentControls.FlowDocumentControls
{
    public class EnhancedFlowDocumentReader : FlowDocumentReader, IEnhancedFlowDocumentControl
    {
        private readonly FindToolBarManager _findToolBarManager = new FindToolBarManager();
        private Decorator _contentHost;

        FindToolBarManager IEnhancedFlowDocumentControl.FindToolBarManager => _findToolBarManager;

        public static readonly DependencyProperty FindToolBarProperty =
            DependencyProperty.Register(
                nameof(FindToolBar),
                typeof(FrameworkElement),
                typeof(EnhancedFlowDocumentReader),
                new PropertyMetadata(null, FindToolBarChanged));

        private static void FindToolBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is EnhancedFlowDocumentReader reader))
            {
                return;
            }

            reader._findToolBarManager.FindToolBarChanged(e.NewValue as FrameworkElement);
        }

        public FrameworkElement FindToolBar
        {
            get => (FrameworkElement)GetValue(FindToolBarProperty);
            set => SetValue(FindToolBarProperty, value);
        }

        #region RetainFindToolBarSettings
        public bool RetainFindToolBarSettings
        {
            get => (bool)GetValue(RetainFindToolBarSettingsProperty);
            set => SetValue(RetainFindToolBarSettingsProperty, value);
        }

        public static readonly DependencyProperty RetainFindToolBarSettingsProperty =
            DependencyProperty.Register(nameof(RetainFindToolBarSettings), typeof(bool), typeof(EnhancedFlowDocumentReader), new PropertyMetadata(false, RetainFindToolBarSettingsChanged));

        private static void RetainFindToolBarSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is EnhancedFlowDocumentReader reader))
            {
                return;
            }

            reader._findToolBarManager.RetainSettings = (bool)e.NewValue;
        }

        #endregion

        #region VerticalScrollbarVisibility

        public ScrollBarVisibility VerticalScrollbarVisibility
        {
            get => (ScrollBarVisibility)GetValue(VerticalScrollbarVisibilityProperty);
            set => SetValue(VerticalScrollbarVisibilityProperty, value);
        }

        public static readonly DependencyProperty VerticalScrollbarVisibilityProperty =
            DependencyProperty.Register(nameof(VerticalScrollbarVisibility), typeof(ScrollBarVisibility), typeof(EnhancedFlowDocumentReader), new PropertyMetadata(ScrollBarVisibility.Visible, VerticalScrollbarVisibilityChanged));

        private static void VerticalScrollbarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => (d as EnhancedFlowDocumentReader).TrySetVerticalScrollbarVisibility((ScrollBarVisibility)e.NewValue);

        private void TrySetVerticalScrollbarVisibility(ScrollBarVisibility scrollBarVisibility)
        {
            if (_contentHost == null || ViewingMode != FlowDocumentReaderViewingMode.Scroll)
            {
                return;
            }

            SetVerticalScrollbarVisibility(scrollBarVisibility);
        }

        private void SetVerticalScrollbarVisibility(ScrollBarVisibility scrollBarVisibility)
        {
            if (!(_contentHost.Child is FlowDocumentScrollViewer flowDocumentScrollViewer))
            {
                return;
            }

            flowDocumentScrollViewer.VerticalScrollBarVisibility = scrollBarVisibility;
        }

        protected override void SwitchViewingModeCore(FlowDocumentReaderViewingMode viewingMode)
        {
            base.SwitchViewingModeCore(viewingMode);
            if (viewingMode != FlowDocumentReaderViewingMode.Scroll)
            {
                return;
            }

            TrySetVerticalScrollbarVisibility(VerticalScrollbarVisibility);
        }

        private void SetContentHost() => _contentHost = GetTemplateChild("PART_ContentHost") as Decorator;

        #endregion

        static EnhancedFlowDocumentReader() => EventManager.RegisterClassHandler(
                typeof(EnhancedFlowDocumentReader),
                Keyboard.KeyDownEvent,
                new KeyEventHandler(FindToolBarManager.KeyDownHandler),
                true);

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _findToolBarManager.Setup(this, FindToolBar);
            SetContentHost();
        }

        protected override void OnKeyDown(KeyEventArgs e) => _findToolBarManager.KeyDown(e, base.OnKeyDown);
    }
}
