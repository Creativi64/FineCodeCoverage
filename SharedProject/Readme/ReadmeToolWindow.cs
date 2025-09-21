using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using EnhancedFlowDocumentControls.FlowDocumentControls;
using FineCodeCoverage.Vs.WindowServices.ToolWindows;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Readme
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("1ee4211e-a350-4092-9d51-d5f15997354c")]
    internal sealed class ReadmeToolWindow
        : ToolWindowPane
    {
        private EnhancedFlowDocumentReader _enhancedFlowDocumentReader;

        private sealed class FlowDocumentReaderShortcut
        {
            public FlowDocumentReaderShortcut(Key key, Keys flag, params Keys[] modifiers)
            {
                Key = key;
                Flag = flag;
                Modifiers = modifiers;
            }

            public Key Key { get; }

            public Keys Flag { get; }

            public Keys[] Modifiers { get; }
        }

        private static readonly IEnumerable<FlowDocumentReaderShortcut> s_flowDocumentReaderShortcuts = new List<FlowDocumentReaderShortcut>()
        {
            new FlowDocumentReaderShortcut(Key.F3, Keys.F3),
            new FlowDocumentReaderShortcut(Key.M, Keys.M, Keys.Control),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadmeToolWindow"/> class.
        /// </summary>
        public ReadmeToolWindow()
            : base(null)
            => Initialize(
                ReflectionMEFToolWindowContextProvider.GetToolWindowContext<ReadmeToolWindow, ReadmeToolWindowContext>());

        public ReadmeToolWindow(ReadmeToolWindowContext context)
            : base(null)
            => Initialize(context);

        private void Initialize(ReadmeToolWindowContext context)
        {
            Caption = "Readme";
            /*
                https://learn.microsoft.com/en-us/visualstudio/extensibility/image-service-and-catalog?view=vs-2022#how-do-i-use-image-monikers-in-a-new-tool-window
                Note that will only be seen when tabs are too small when docked
                when Ctrl + Tab switching
                drop down selector MDI
            */

            BitmapImageMoniker = Microsoft.VisualStudio.Imaging.KnownMonikers.WelcomeUserGuide;

            _enhancedFlowDocumentReader = new EnhancedFlowDocumentReader
            {
                Document = context.ReadMeMarkdownViewModel.FlowDocument,
                ViewingMode = FlowDocumentReaderViewingMode.Scroll,
                DataContext = context.ReadMeMarkdownViewModel,
            };
            Content = _enhancedFlowDocumentReader;
        }

        protected override bool PreProcessMessage(
            ref Message m)
        {
            if (!(Content is EnhancedFlowDocumentReader target && target.IsKeyboardFocusWithin && m.IsKeyUpOrDown() is bool isKeyDown))
            {
                return base.PreProcessMessage(ref m);
            }

            foreach (FlowDocumentReaderShortcut flowDocumentReaderShortcut in s_flowDocumentReaderShortcuts)
            {
                if (m.HasFlag(flowDocumentReaderShortcut.Flag) && flowDocumentReaderShortcut.Modifiers.All(modifier => System.Windows.Forms.Control.ModifierKeys.HasFlag(modifier)))
                {
                    System.Windows.Input.KeyEventArgs keyEventArgs = CreateKeyEventArgs(flowDocumentReaderShortcut.Key, isKeyDown);
                    _ = InputManager.Current.ProcessInput(keyEventArgs);
                    return true;
                }
            }

            return base.PreProcessMessage(ref m);
        }

        private static System.Windows.Input.KeyEventArgs CreateKeyEventArgs(Key key, bool isDown)
        {
            KeyboardDevice keyboardDevice = InputManager.Current.PrimaryKeyboardDevice;

            return new System.Windows.Input.KeyEventArgs(
                keyboardDevice,
                keyboardDevice.ActiveSource,
                0,
                key)
            {
                RoutedEvent = isDown ? Keyboard.KeyDownEvent : Keyboard.KeyUpEvent,
            };
        }
    }
}
