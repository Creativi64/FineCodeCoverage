using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    public abstract class VisualStudioTreeItemBase : TreeItemBase
    {
        public override abstract bool IsExpanded { get; set; }
        protected override Brush SelectedActiveBackgroundBrush => ThemedTreeGridColours.Instance.SelectedItemActiveBackColor;
        protected override Brush SelectedInactiveBackgroundBrush => ThemedTreeGridColours.Instance.SelectedItemInactiveBackColor;
        protected override Brush SelectedActiveForegroundBrush => ThemedTreeGridColours.Instance.SelectedItemActiveForeColor;
        protected override Brush SelectedInactiveForegroundBrush => ThemedTreeGridColours.Instance.SelectedItemInactiveForeColor;
        protected override Brush NotSelectedForegroundBrush => ThemedTreeGridColours.Instance.ForegroundColor;
        public VisualStudioTreeItemBase() => this.SetupThemeChange();

        private void SetupThemeChange() 
            => VSColorTheme.ThemeChanged += (ThemeChangedEventHandler)(e =>
            {
                ThemedTreeGridColours.Instance.VSColorTheme_ThemeChanged(e);
                this.OnPropertyChanged("Background");
                this.OnPropertyChanged("Foreground");
            });
    }
}
