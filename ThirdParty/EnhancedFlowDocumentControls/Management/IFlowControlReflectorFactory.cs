namespace EnhancedFlowDocumentControls.Management
{
    internal interface IFlowControlReflectorFactory
    {
        IFlowControlReflector GetReflector(IEnhancedFlowDocumentControl enhancedFlowControl);
    }
}
