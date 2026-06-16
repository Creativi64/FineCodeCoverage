using System;
using System.Windows;

namespace EnhancedFlowDocumentControls.Management
{
    internal interface IWpfUtilities
    {
        Action<Action> Dispatcher { get; set; }

        void AddPreviewKeyDownEnterOrExecuteHandler(Action handler);

        void Clear();

        void FindTextBox(FrameworkElement customFindToolBar, Action callback);

        void FocusTextBox();

        string GetFocusedElementName(DependencyObject element);

        void TryDispatchSetFocusedCustomFindToolBarDescendentByName(DependencyObject element, string focusedElementName);
    }
}
