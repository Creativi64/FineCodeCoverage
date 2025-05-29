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
        private readonly ThemedTreeGridColours themedTreeGridColours = ThemedTreeGridColours.Instance;
        protected override Brush SelectedActiveBackgroundBrush => this.themedTreeGridColours.SelectedItemActiveBackColor;
        protected override Brush SelectedInactiveBackgroundBrush => this.themedTreeGridColours.SelectedItemInactiveBackColor;
        protected override Brush SelectedActiveForegroundBrush => this.themedTreeGridColours.SelectedItemActiveForeColor;
        protected override Brush SelectedInactiveForegroundBrush => this.themedTreeGridColours.SelectedItemInactiveForeColor;
        protected override Brush NotSelectedForegroundBrush => this.themedTreeGridColours.ForegroundColor;
        protected VisualStudioTreeItemBase() => ThemeChangedWeakEventManager.AddListener(this);

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            this.NotifyForegroundBackgroundChanged();
            return true;
        }
    }
}
