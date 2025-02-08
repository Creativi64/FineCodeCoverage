using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Windows;

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
