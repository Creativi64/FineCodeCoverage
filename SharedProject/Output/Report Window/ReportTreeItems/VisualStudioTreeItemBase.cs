using System;
using System.Windows;
using System.Windows.Media;
using FineCodeCoverage.Core.Utilities;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    public abstract class VisualStudioTreeItemBase : TreeItemBase, IWeakEventListener
    {
        // so the instance handles the ThemeChanged event first
        private ThemedTreeGridColours themedTreeGridColours = ThemedTreeGridColours.Instance;
        protected override Brush SelectedActiveBackgroundBrush => themedTreeGridColours.SelectedItemActiveBackColor;
        protected override Brush SelectedInactiveBackgroundBrush => themedTreeGridColours.SelectedItemInactiveBackColor;
        protected override Brush SelectedActiveForegroundBrush => themedTreeGridColours.SelectedItemActiveForeColor;
        protected override Brush SelectedInactiveForegroundBrush => themedTreeGridColours.SelectedItemInactiveForeColor;
        protected override Brush NotSelectedForegroundBrush => themedTreeGridColours.ForegroundColor;
        protected VisualStudioTreeItemBase() => ThemeChangedWeakEventManager.AddListener(this);

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            this.NotifyForegroundBackgroundChanged();
            return true;
        }
    }
}
