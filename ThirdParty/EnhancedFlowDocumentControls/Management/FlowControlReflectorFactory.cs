using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace EnhancedFlowDocumentControls.Management
{
    internal sealed class FlowControlReflectorFactory : IFlowControlReflectorFactory
    {
        private static readonly Dictionary<Type, FlowControlReflector> s_flowControlReflectorLookup;

        static FlowControlReflectorFactory()
        {
            var types = new List<Type> { typeof(FlowDocumentPageViewer), typeof(FlowDocumentScrollViewer), typeof(FlowDocumentReader) };
            s_flowControlReflectorLookup = types.ToDictionary(t => t, t => new FlowControlReflector(t));
        }

        public IFlowControlReflector GetReflector(IEnhancedFlowDocumentControl enhancedFlowControl)
        {
            Type type = enhancedFlowControl.GetType();
            return s_flowControlReflectorLookup.First(kvp => type.IsSubclassOf(kvp.Key)).Value;
        }
    }
}
