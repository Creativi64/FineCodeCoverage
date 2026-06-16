using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using EnhancedFlowDocumentControls.Utils;

namespace EnhancedFlowDocumentControls.Management
{
    internal sealed class WpfUtilities : IWpfUtilities
    {
        internal const string FindTextBoxName = "findTextBox";
        private FrameworkElement _customFindToolBar;
        private RoutedEventHandler _loadedHandler;
        private TextBox _findTextBox;
        private KeyEventHandler _previewKeyDownHandler;

        public Action<Action> Dispatcher { get; set; }

        public void AddPreviewKeyDownEnterOrExecuteHandler(Action handler)
        {
            _previewKeyDownHandler = (_, e) =>
            {
                if (e == null || (e.Key != Key.Return && e.Key != Key.Execute))
                {
                    return;
                }

                e.Handled = true;
                handler();
            };

            _findTextBox.PreviewKeyDown += _previewKeyDownHandler;
        }

        public void FocusTextBox()
            => DoDispatch(
                () =>
                {
                    // due to fast toggling of the find toolbar, the textbox may not be there
                    if (_findTextBox == null)
                    {
                        return;
                    }

                    _ = _findTextBox.Focus();
                    _ = Keyboard.Focus(_findTextBox);
                });

        private void DoDispatch(Action action)
        {
            if (Dispatcher != null)
            {
                Dispatcher(action);
            }
            else
            {
                _ = _customFindToolBar.Dispatcher.BeginInvoke(action, DispatcherPriority.Background);
            }
        }

        private void AddLoadedEventHandler(FrameworkElement customFindToolBar, RoutedEventHandler loadedHandler)
        {
            _customFindToolBar = customFindToolBar;
            _loadedHandler = loadedHandler;
            customFindToolBar.Loaded += loadedHandler;
        }

        public void Clear()
        {
            if (_findTextBox != null)
            {
                _findTextBox.PreviewKeyDown -= _previewKeyDownHandler;
                _previewKeyDownHandler = null;
                _findTextBox = null;
            }

            _customFindToolBar.Loaded -= _loadedHandler;
            _loadedHandler = null;
            _customFindToolBar = null;
        }

        public void FindTextBox(FrameworkElement customFindToolBar, Action callback)
            => AddLoadedEventHandler(customFindToolBar, (_, __) => DoDispatch(
                () =>
                {
                    _findTextBox = VisualTreeUtilities.FindByName<TextBox>(_customFindToolBar, FindTextBoxName);
                    if (_findTextBox == null)
                    {
                        return;
                    }

                    callback();
                }));

        public string GetFocusedElementName(DependencyObject element)
            => FocusManager.GetFocusedElement(element) is FrameworkElement focusedElement ? focusedElement.Name : null;

        public void TryDispatchSetFocusedCustomFindToolBarDescendentByName(DependencyObject element, string focusedElementName)
        {
            FrameworkElement newFocusedElement = VisualTreeUtilities.FindByName<FrameworkElement>(_customFindToolBar, focusedElementName);
            if (newFocusedElement == null)
            {
                return;
            }

            DoDispatch(() => FocusManager.SetFocusedElement(element, newFocusedElement));
        }
    }
}
