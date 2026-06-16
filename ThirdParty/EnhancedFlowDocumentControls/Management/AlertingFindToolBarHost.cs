using System;
using System.Windows;
using System.Windows.Controls;

namespace EnhancedFlowDocumentControls.Management
{
    internal sealed class AlertingFindToolBarHost : Decorator
    {
        private ToolBar _findToolBar;

        public event EventHandler<ToolBar> ShowToolBarEvent;

        public event EventHandler CloseToolBarEvent;

        public override UIElement Child
        {
            get => _findToolBar;
            set
            {
                if (value == null)
                {
                    CloseToolBarEvent?.Invoke(this, EventArgs.Empty);
                    _findToolBar = null;
                }
                else if (value is ToolBar findToolBar)
                {
                    _findToolBar = findToolBar;
                    ShowToolBarEvent?.Invoke(this, findToolBar);
                }
                else
                {
                    throw new Exception("Unexpected value for ShimFindToolBarHost.Child. Expected null or ToolBar, but got: " + value.GetType().Name);
                }
            }
        }
    }
}
