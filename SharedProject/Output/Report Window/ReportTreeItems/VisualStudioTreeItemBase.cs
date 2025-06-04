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
        private readonly ThemedTreeGridColours _themedTreeGridColours = ThemedTreeGridColours.Instance;
        protected override Brush SelectedActiveBackgroundBrush => this._themedTreeGridColours.SelectedItemActiveBackColor;
        protected override Brush SelectedInactiveBackgroundBrush => this._themedTreeGridColours.SelectedItemInactiveBackColor;
        protected override Brush SelectedActiveForegroundBrush => this._themedTreeGridColours.SelectedItemActiveForeColor;
        protected override Brush SelectedInactiveForegroundBrush => this._themedTreeGridColours.SelectedItemInactiveForeColor;
        protected override Brush NotSelectedForegroundBrush => this._themedTreeGridColours.ForegroundColor;
        protected VisualStudioTreeItemBase() => ThemeChangedWeakEventManager.AddListener(this);

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            this.NotifyForegroundBackgroundChanged();
            return true;
        }
    }
}
