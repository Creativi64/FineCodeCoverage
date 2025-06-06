using System;
using System.Runtime.InteropServices;
using FineCodeCoverage.Output;
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

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            Content = new ReadmeControl()
            {
                DataContext = context.ReadMeMarkdownViewModel,
            };
        }
    }
}
