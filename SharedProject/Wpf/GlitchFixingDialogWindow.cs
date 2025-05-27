using System;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Wpf
{
    public abstract class GlitchFixingDialogWindow : DialogWindow
    {
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (this.SizeToContent == SizeToContent.WidthAndHeight)
            {
                InvalidateMeasure();
            }
        }
    }
}
