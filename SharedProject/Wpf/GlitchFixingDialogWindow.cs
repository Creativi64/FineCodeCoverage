using System;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Wpf
{
    public abstract class GlitchFixingDialogWindow : DialogWindow
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.FixSizeToContentWidthAndHeightBlackBars();
        }
    }
}
