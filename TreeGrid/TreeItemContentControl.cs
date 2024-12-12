using System.Windows;
using System.Windows.Controls;

namespace TreeGrid
{
    public class TreeItemContentControl : ContentControl
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var contentPresenter = GetContentPresenter();
            contentPresenter.Loaded += ContentPresenter_Loaded;
        }

        private void ContentPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            var cp = sender as ContentPresenter;
            NotifyTreeGridControl(cp);
            cp.Loaded -= ContentPresenter_Loaded;
        }

        private void NotifyTreeGridControl(ContentPresenter contentPresenter)
        {
            contentPresenter.GetAscendant<TreeGridControl>().TreeItemContentControlLoaded(contentPresenter);
        }
        
        private ContentPresenter GetContentPresenter() => this.FindVisualChild<ContentPresenter>();

    }
}
