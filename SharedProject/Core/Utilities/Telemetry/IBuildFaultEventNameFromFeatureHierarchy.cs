namespace FineCodeCoverage.Core.Utilities.Telemetry
{
    internal interface IBuildFaultEventNameFromFeatureHierarchy
    {
        FaultEventName BuildFromFeatureNameHierarchy(params string[] hierarchy);
    }
}
