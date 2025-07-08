using System;
using System.Windows;

namespace FineCodeCoverage.Wpf
{
    /*
        https://stackoverflow.com/questions/29207331/wpf-window-with-custom-chrome-has-unwanted-outline-on-right-and-bottom/51573942#51573942
        https://stackoverflow.com/questions/57022923/wpf-windowchrome-unwanted-black-shadow-caused-by-sizetocontent-widthandheight
        https://stackoverflow.com/questions/37333952/wpf-sizetocontent-widthandheight-windowstate-minimized-bug
    */
    public static class SizeToContentWidthAndHeightFixer
    {
        public static void FixSizeToContentWidthAndHeightBlackBars(this Window window)
        {
            if (window.SizeToContent != SizeToContent.WidthAndHeight)
            {
                return;
            }

            void Window_SourceInitialized(object sender, EventArgs e)
            {
                window.InvalidateMeasure();
                window.SourceInitialized -= Window_SourceInitialized;
            }

            window.SourceInitialized += Window_SourceInitialized;
        }
    }
}
