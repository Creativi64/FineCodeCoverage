using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FineCodeCoverage.Wpf
{
    public static class WindowCloseViewModelBehaviour
    {
        public static bool GetEnabled(DependencyObject obj) => (bool)obj.GetValue(EnabledProperty);

        public static void SetEnabled(DependencyObject obj, bool value) => obj.SetValue(EnabledProperty, value);

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(WindowCloseViewModelBehaviour), new PropertyMetadata(false, OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Window window))
            {
                return;
            }

            if (window.IsLoaded)
            {
                AddBehaviour(window);
            }
            else
            {
                window.Loaded += (sender, args) => AddBehaviour(window);
            }
        }

        private static void AddBehaviour(Window window)
        {
            bool viewModelClosed = false;
            EventHandler<bool> viewModelDoneHandler = null;
            KeyEventHandler notifyViewModelEscapeKeyDown = null;
            EventHandler notifyViewModelCloseHandler = null;

            window.Closed += (sender, args) =>
            {
                RemoveNotifyingViewModelHandlers();
                RemoveViewModelDoneHandler();
            };

            void RemoveNotifyingViewModelHandlers()
            {
                if (notifyViewModelCloseHandler != null)
                {
                    window.Closed -= notifyViewModelCloseHandler;
                    notifyViewModelCloseHandler = null;
                }

                if (notifyViewModelEscapeKeyDown != null)
                {
                    window.KeyDown -= notifyViewModelEscapeKeyDown;
                    notifyViewModelEscapeKeyDown = null;
                }
            }

            void RemoveViewModelDoneHandler()
            {
                if (viewModelDoneHandler == null || !(window.DataContext is IViewModelDone vmDone))
                {
                    return;
                }

                vmDone.Done -= viewModelDoneHandler;
                viewModelDoneHandler = null;
            }

            if (window.DataContext is IViewModelCancel viewModelClose)
            {
                void NotifyViewModelCloseHandler(object sender, EventArgs args)
                {
                    if (viewModelClosed)
                    {
                        return;
                    }

                    RemoveViewModelDoneHandler();

                    viewModelClose.CancelCommand.Execute(WindowCloseFrom.AltF4);
                }

                notifyViewModelCloseHandler = NotifyViewModelCloseHandler;
                window.Closed += NotifyViewModelCloseHandler;

                void NotifyViewModelEscapeKeyDown(object sender, KeyEventArgs args)
                {
                    if (args.Key != Key.Escape)
                    {
                        return;
                    }

                    viewModelClose.CancelCommand.Execute(WindowCloseFrom.EscapeKey);
                }

                notifyViewModelEscapeKeyDown = NotifyViewModelEscapeKeyDown;
                window.KeyDown += NotifyViewModelEscapeKeyDown;

                TrySetCloseWindowButtonCommand(window, viewModelClose.CancelCommand);
            }

            if (!(window.DataContext is IViewModelDone viewModelDone))
            {
                return;
            }

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
            void ViewModelDoneHandler(object _, bool ok)
            {
                viewModelClosed = true;
                window.DialogResult = ok;
                window.Close();
            }
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

            viewModelDoneHandler = ViewModelDoneHandler;
            viewModelDone.Done += ViewModelDoneHandler;
        }

        private static void TrySetCloseWindowButtonCommand(Window window, ICommand command)
        {
            Button button = VisualTreeUtilties.FindByName<Button>(window, "DialogCloseButton");
            if (button == null)
            {
                return;
            }

            button.Command = command;
            button.CommandParameter = WindowCloseFrom.XButton;
        }
    }
}
