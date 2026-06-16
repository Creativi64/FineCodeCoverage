using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnhancedFlowDocumentControls.Management
{
    internal sealed class DocumentViewerHelper : IDocumentViewerHelper
    {
        private static readonly MethodInfo s_keyDownHelperMethod;

        static DocumentViewerHelper()
        {
            Type documentViewHelperType = typeof(FlowDocumentPageViewer).Assembly.GetType("MS.Internal.Documents.DocumentViewerHelper");
            s_keyDownHelperMethod = documentViewHelperType.GetMethod("KeyDownHelper", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public void KeyDownHelper(KeyEventArgs e, DependencyObject findToolBarHost)
            => s_keyDownHelperMethod.Invoke(null, new object[] { e, findToolBarHost });

        // relevnt parts of MS.Internal.Documents.DocumentViewerHelper.ToggleFindToolBar
        public void ToggleFindToolBarHost(Decorator findToolBarHost, bool showToolBar)
        {
            if (showToolBar)
            {
                findToolBarHost.Visibility = Visibility.Visible;
                KeyboardNavigation.SetTabNavigation(findToolBarHost, KeyboardNavigationMode.Continue);
                FocusManager.SetIsFocusScope(findToolBarHost, true);
            }
            else
            {
                findToolBarHost.Visibility = Visibility.Collapsed;
                KeyboardNavigation.SetTabNavigation(findToolBarHost, KeyboardNavigationMode.None);
                findToolBarHost.ClearValue(FocusManager.IsFocusScopeProperty);
            }
        }
    }
}
