using System;
using System.Reflection;
using System.Windows.Controls;

namespace EnhancedFlowDocumentControls.Management
{
    internal sealed class FlowControlReflector : IFlowControlReflector
    {
        private readonly PropertyInfo _canShowFindToolBarProperty;

        private readonly FieldInfo _findToolBarHostField;

        public FlowControlReflector(Type t)
        {
            _canShowFindToolBarProperty = t.GetProperty("CanShowFindToolBar", BindingFlags.Instance | BindingFlags.NonPublic);
            _findToolBarHostField = t.GetField("_findToolBarHost", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public bool CanShowFindToolBar(object flowControl) => (bool)_canShowFindToolBarProperty.GetValue(flowControl);

        public Decorator GetFindToolBarHost(object flowControl) => _findToolBarHostField.GetValue(flowControl) as Decorator;

        public void SetFindToolBarHost(object flowControl, Decorator value) => _findToolBarHostField.SetValue(flowControl, value);
    }
}
