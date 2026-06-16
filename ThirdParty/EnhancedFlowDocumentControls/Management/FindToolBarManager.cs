using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnhancedFlowDocumentControls.ViewModel;

namespace EnhancedFlowDocumentControls.Management
{
    internal sealed class FindToolBarManager
    {
        private readonly AlertingFindToolBarHost _alertingFindToolBarHost;
        private readonly IFlowControlReflectorFactory _flowControlReflectorFactory;
        private readonly IDocumentViewerHelper _documentViewerHelper;
        private readonly IFindToolBarViewModelFactory _findToolBarViewModelFactory;
        private readonly IWpfUtilities _wpfUtilities;
        private IFlowControlReflector _flowControlReflector;
        private FrameworkElement _customFindToolBar;
        private Decorator _originalFindToolBarHost;
        private IEnhancedFlowDocumentControl _flowControl;
        private IFindableToolBarViewModel _findToolBarViewModel;
        private bool _retainSettings;
        private IFindToolBarSettings _retainedSettings;

        internal FindToolBarManager(Action<Action> dispatcher = null)
            : this(
                  new AlertingFindToolBarHost(),
                  new FlowControlReflectorFactory(),
                  new DocumentViewerHelper(),
                  new FindToolBarViewModelFactory(),
                  new WpfUtilities(),
                  dispatcher)
        {
        }

        // ctor for tests
        internal FindToolBarManager(
            AlertingFindToolBarHost alertingFindToolBarHost,
            IFlowControlReflectorFactory flowControlReflectorFactory,
            IDocumentViewerHelper documentViewerHelper,
            IFindToolBarViewModelFactory findToolBarViewModelFactory,
            IWpfUtilities wpfUtilities,
            Action<Action> dispatcher = null)
        {
            wpfUtilities.Dispatcher = dispatcher;
            _alertingFindToolBarHost = alertingFindToolBarHost;
            _flowControlReflectorFactory = flowControlReflectorFactory;
            _documentViewerHelper = documentViewerHelper;
            _findToolBarViewModelFactory = findToolBarViewModelFactory;
            _wpfUtilities = wpfUtilities;
            alertingFindToolBarHost.ShowToolBarEvent += AlertingFindToolBarHost_ShowToolBarEvent;
            alertingFindToolBarHost.CloseToolBarEvent += AlertingFindToolBarHost_CloseToolBarEvent;
        }

        public bool RetainSettings
        {
            get => _retainSettings;
            set
            {
                _retainSettings = value;
                if (_retainSettings)
                {
                    return;
                }

                _retainedSettings = null;
            }
        }

        private bool IsShowingFindToolbar => _originalFindToolBarHost.Child != null;

        internal void Setup(IEnhancedFlowDocumentControl flowControl, FrameworkElement customFindToolBar)
        {
            _customFindToolBar = customFindToolBar;
            _flowControl = flowControl;
            ReplaceToolBarHostFieldWithAlertingNotInTree();
        }

        private void ReplaceToolBarHostFieldWithAlertingNotInTree()
        {
            _flowControlReflector = _flowControlReflectorFactory.GetReflector(_flowControl);
            _originalFindToolBarHost = _flowControlReflector.GetFindToolBarHost(_flowControl);
            _flowControlReflector.SetFindToolBarHost(_flowControl, _alertingFindToolBarHost);
        }

        private void AlertingFindToolBarHost_CloseToolBarEvent(object sender, EventArgs e)
        {
            if (RetainSettings)
            {
                _retainedSettings = FindToolBarSettings.Clone(_findToolBarViewModel);
            }

            _findToolBarViewModel = null;

            _originalFindToolBarHost.Child = null;
            _wpfUtilities.Clear();
            _documentViewerHelper.ToggleFindToolBarHost(_originalFindToolBarHost, false);
        }

        private void AlertingFindToolBarHost_ShowToolBarEvent(object sender, ToolBar findToolBar)
        {
            FrameworkElement originalDataContextElement = _customFindToolBar is IFindToolBarViewModelAware ? null : _flowControl as FrameworkElement;
            _findToolBarViewModel = _findToolBarViewModelFactory.Create(findToolBar, originalDataContextElement);
            if (_retainedSettings != null)
            {
                _findToolBarViewModel.ApplySettings(_retainedSettings);
            }

            AddCustomFindToolBarToHost();
            _documentViewerHelper.ToggleFindToolBarHost(_originalFindToolBarHost, true);
            _wpfUtilities.FindTextBox(_customFindToolBar, ReadyTextBox);
        }

        private void AddCustomFindToolBarToHost()
        {
            SetCustomFindToolBarViewModel();
            _originalFindToolBarHost.Child = _customFindToolBar;
        }

        private void SetCustomFindToolBarViewModel()
        {
            if (_customFindToolBar is IFindToolBarViewModelAware findToolBarViewModelAware)
            {
                findToolBarViewModelAware.FindToolBarViewModel = _findToolBarViewModel;
            }
            else
            {
                _customFindToolBar.DataContext = _findToolBarViewModel;
            }
        }

        private void ReadyTextBox()
        {
            _wpfUtilities.AddPreviewKeyDownEnterOrExecuteHandler(_findToolBarViewModel.Find);
            _wpfUtilities.FocusTextBox();
        }

        // the requirement to not call base is due to FlowDocumentPageViewer not immediately exiting when e.Handled is true - it still processes F3 itself
        internal void KeyDown(KeyEventArgs e, Action<KeyEventArgs> baseKeyDown)
        {
            if (IsShowingFindToolbar && e.Key == Key.F3)
            {
                bool shiftPressed = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                _findToolBarViewModel.Find(shiftPressed);
                e.Handled = true;
                return;
            }

            baseKeyDown(e);
        }

        private void KeyDownHandler(KeyEventArgs e) => _documentViewerHelper.KeyDownHelper(e, _originalFindToolBarHost);

        internal static void KeyDownHandler(object sender, KeyEventArgs e)
        {
            FindToolBarManager findToolBarManager = (sender as IEnhancedFlowDocumentControl).FindToolBarManager;
            findToolBarManager.KeyDownHandler(e);
        }

        // unlikely and not documented.
        internal void FindToolBarChanged(FrameworkElement customFindToolBar)
        {
            _customFindToolBar = customFindToolBar;
            if (_findToolBarViewModel == null)
            {
                return;
            }

            string focusedElementName = _wpfUtilities.GetFocusedElementName(_originalFindToolBarHost);

            AddCustomFindToolBarToHost();

            _wpfUtilities.FindTextBox(_customFindToolBar, () =>
            {
                _wpfUtilities.AddPreviewKeyDownEnterOrExecuteHandler(_findToolBarViewModel.Find);

                if (focusedElementName == null)
                {
                    return;
                }

                _wpfUtilities.TryDispatchSetFocusedCustomFindToolBarDescendentByName(_originalFindToolBarHost, focusedElementName);
            });
        }
    }
}
