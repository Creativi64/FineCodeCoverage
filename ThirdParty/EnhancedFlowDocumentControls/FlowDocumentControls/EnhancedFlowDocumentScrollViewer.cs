using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnhancedFlowDocumentControls.Management;

namespace EnhancedFlowDocumentControls.FlowDocumentControls
{
    public class EnhancedFlowDocumentScrollViewer : FlowDocumentScrollViewer, IEnhancedFlowDocumentControl
    {
        private readonly FindToolBarManager _findToolBarManager = new FindToolBarManager();

        FindToolBarManager IEnhancedFlowDocumentControl.FindToolBarManager => _findToolBarManager;

        public static readonly DependencyProperty FindToolBarProperty =
            DependencyProperty.Register(
                nameof(FindToolBar),
                typeof(FrameworkElement),
                typeof(EnhancedFlowDocumentScrollViewer),
                new PropertyMetadata(FindToolBarChanged));

        private static void FindToolBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is EnhancedFlowDocumentScrollViewer viewer))
            {
                return;
            }

            viewer._findToolBarManager.FindToolBarChanged(e.NewValue as FrameworkElement);
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
            DependencyProperty.Register(nameof(RetainFindToolBarSettings), typeof(bool), typeof(EnhancedFlowDocumentScrollViewer), new PropertyMetadata(false, RetainFindToolBarSettingsChanged));

        private static void RetainFindToolBarSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is EnhancedFlowDocumentScrollViewer viewer))
            {
                return;
            }

            viewer._findToolBarManager.RetainSettings = (bool)e.NewValue;
        }

        #endregion

        static EnhancedFlowDocumentScrollViewer() => EventManager.RegisterClassHandler(
                typeof(EnhancedFlowDocumentScrollViewer),
                Keyboard.KeyDownEvent,
                new KeyEventHandler(FindToolBarManager.KeyDownHandler),
                true);

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _findToolBarManager.Setup(this, FindToolBar);
        }

        protected override void OnKeyDown(KeyEventArgs e) => _findToolBarManager.KeyDown(e, base.OnKeyDown);
    }
}
